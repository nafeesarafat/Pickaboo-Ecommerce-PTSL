using System.Text;
using System.Text.Json;
using Ecommerce.Api.Configurations;
using Ecommerce.Api.Data;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Middleware;
using Ecommerce.Api.Repositories;
using Ecommerce.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------- configuration
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<PasswordResetOptions>(builder.Configuration.GetSection(PasswordResetOptions.SectionName));
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.AddOptions<EmailOptions>()
    .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => options.EnableSsl, "'Email:EnableSsl' must be true for Gmail SMTP.")
    .ValidateOnStart();

var databaseOptions = builder.Configuration
    .GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
    ?? throw new InvalidOperationException("The 'ConnectionStrings' configuration section is missing.");

if (string.IsNullOrWhiteSpace(databaseOptions.DefaultConnection))
{
    throw new InvalidOperationException(
        "No database connection string is configured. Set ConnectionStrings:DefaultConnection in " +
        "appsettings.Development.json or the environment variable ConnectionStrings__DefaultConnection.");
}

var corsOptions = builder.Configuration
    .GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

// ------------------------------------------------------------------------ DI
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        databaseOptions.DefaultConnection,
        sqlServer => sqlServer.EnableRetryOnFailure()));

// Application services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<AdminSeeder>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEmailSender, GmailEmailSender>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAdminOrderQueryService, AdminOrderQueryService>();
builder.Services.AddScoped<IAdminReviewService, AdminReviewService>();
builder.Services.AddScoped<IAdminPaymentMethodService, AdminPaymentMethodService>();
builder.Services.AddScoped<IAdminShippingService, AdminShippingService>();
builder.Services.AddScoped<IAdminOrderQueryRepository, AdminOrderQueryRepository>();
builder.Services.AddScoped<IAdminCustomerService, AdminCustomerService>();
builder.Services.AddScoped<IAdminCustomerRepository, AdminCustomerRepository>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IAdminBrandService, AdminBrandService>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IAdminInventoryService, AdminInventoryService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();
builder.Services.AddScoped<IStoreSettingsService, StoreSettingsService>();
builder.Services.AddScoped<IAdminActivityLogService, AdminActivityLogService>();
builder.Services.AddScoped<IExtendedAuthService, ExtendedAuthService>();
builder.Services.AddDataProtection();

// Lets CurrentUserService read the validated identity of the current request
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

// Validation failures are returned in the same { success, message, errors } shape as every
// other error instead of the framework default ValidationProblemDetails.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "The value provided is not valid."
                    : error.ErrorMessage))
            .ToList();

        var payload = ApiErrorResponse.Create("One or more validation errors occurred.", errors);

        return new BadRequestObjectResult(payload)
        {
            ContentTypes = { "application/json" }
        };
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsOptions.PolicyName, policy =>
    {
        if (corsOptions.AllowedOrigins.Length == 0)
        {
            // No origin configured: nothing is allowed, which is the safe default for a new project.
            policy.WithOrigins(Array.Empty<string>());
        }
        else
        {
            policy.WithOrigins(corsOptions.AllowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

// ------------------------------------------------------------------- security
// JWT infrastructure only. The login/register APIs that issue the tokens come in the next phase.
if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    throw new InvalidOperationException(
        "No JWT signing key is configured. Set Jwt:Key in appsettings (or the environment variable " +
        "Jwt__Key). Never hard-code a real secret in source control.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // Answer a rejected token with the same error shape the rest of the API uses.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                string? sessionClaim = context.Principal?.FindFirst("sid")?.Value;
                if (sessionClaim is null)
                {
                    return;
                }

                if (!int.TryParse(sessionClaim, out int sessionId))
                {
                    context.Fail("The access token session is invalid.");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                bool active = await dbContext.Sessions
                    .AsNoTracking()
                    .AnyAsync(session => session.SessionId == sessionId && session.IsActive);
                if (!active)
                {
                    context.Fail("The access token session is no longer active.");
                }
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();

                if (context.Response.HasStarted)
                {
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var payload = ApiErrorResponse.Create(
                    context.AuthenticateFailure is null
                        ? "Authentication is required."
                        : "The access token is missing, invalid or expired.");

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload, ApiJson.SerializerOptions));
            },
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted)
                {
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var payload = ApiErrorResponse.Create("You do not have permission to perform this action.");
                await context.Response.WriteAsync(JsonSerializer.Serialize(payload, ApiJson.SerializerOptions));
            }
        };
    });

builder.Services.AddAuthorization();

// -------------------------------------------------------------------- Swagger
builder.Services.AddEcommerceSwagger();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    _ = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
    await adminSeeder.SeedAsync();
}

// Registered first so that it can catch failures raised by everything below it.
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseEcommerceSwagger();
}

app.UseHttpsRedirection();
app.UseCors(corsOptions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
