using Microsoft.OpenApi.Models;

namespace Ecommerce.Api.Helpers;

/// <summary>
/// Swagger/OpenAPI configuration, including the "Authorize" button that lets a JWT bearer
/// token be attached to requests once the authentication APIs are implemented.
/// </summary>
public static class SwaggerExtensions
{
    public static void AddEcommerceSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ecommerce API",
                Version = "v1",
                Description = "Backend API for the ecommerce platform. APIs are added phase by phase."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the JWT token as: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    public static void UseEcommerceSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API v1");
            options.DocumentTitle = "Ecommerce API";
        });
    }
}
