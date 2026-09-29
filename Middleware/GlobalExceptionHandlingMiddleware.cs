using System.Text.Json;
using Ecommerce.Api.Helpers;
using Ecommerce.Api.Data;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Middleware;

/// <summary>
/// Centralized exception handling. Converts unhandled exceptions into a consistent
/// { success, message, errors } payload and never leaks stack traces to the client.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client went away - there is nobody left to answer.
            _logger.LogInformation("Request {Path} was cancelled by the client.", context.Request.Path);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception, dbContext);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, AppDbContext dbContext)
    {
        // Expected business failures are not bugs: they are logged as warnings and answered with the
        // message the service chose, never with exception internals.
        if (exception is ApiException apiException)
        {
            _logger.LogWarning(
                "{ExceptionType} on {Method} {Path}: {Message}",
                apiException.GetType().Name,
                context.Request.Method,
                context.Request.Path,
                apiException.Message);

            var apiPayload = ApiErrorResponse.Create(apiException.Message);

            if (exception is BadRequestException badRequest)
            {
                apiPayload.Errors.AddRange(badRequest.Errors);
            }

            await WriteAsync(context, apiException.StatusCode, apiPayload);
            return;
        }

        _logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}",
            context.Request.Method,
            context.Request.Path);
        await RecordSystemErrorAsync(context, exception, dbContext);

        if (context.Response.HasStarted)
        {
            // Headers are already on the wire, so the response cannot be replaced.
            _logger.LogWarning("Response already started; the error payload could not be written.");
            return;
        }

        var (statusCode, payload) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden,
                ApiErrorResponse.Create("You do not have permission to perform this action.")),

            ArgumentException => (StatusCodes.Status400BadRequest,
                ApiErrorResponse.Create("The request contains invalid data.")),

            _ => (StatusCodes.Status500InternalServerError,
                ApiErrorResponse.Create("Something went wrong."))
        };

        // Full details are only attached outside production, to help local debugging.
        if (!_environment.IsProduction())
        {
            payload.Errors.Add(exception.Message);
        }

        await WriteAsync(context, statusCode, payload);
    }

    private async Task RecordSystemErrorAsync(HttpContext context, Exception exception, AppDbContext dbContext)
    {
        try
        {
            dbContext.ChangeTracker.Clear();
            dbContext.SystemLogs.Add(new SystemLog
            {
                Level = "Error",
                Message = $"Unhandled {exception.GetType().Name} in {context.Request.Method} {context.Request.Path}.",
                Timestamp = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception loggingException)
        {
            _logger.LogError(
                loggingException,
                "Failed to persist the system log for {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, ApiErrorResponse payload)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, ApiJson.SerializerOptions));
    }
}
