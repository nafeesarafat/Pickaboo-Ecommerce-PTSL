namespace Ecommerce.Api.Helpers;

/// <summary>
/// Base type for expected, business-level failures. These are converted into the standard
/// error payload by GlobalExceptionHandlingMiddleware - they are not bugs, so they are never
/// logged with a stack trace and never expose internal details to the client.
/// </summary>
public class ApiException : Exception
{
    public ApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}

/// <summary>400 - the request itself is not acceptable.</summary>
public class BadRequestException : ApiException
{
    public BadRequestException(string message, IEnumerable<string>? errors = null)
        : base(message, StatusCodes.Status400BadRequest)
    {
        if (errors is not null)
        {
            Errors.AddRange(errors);
        }
    }

    public List<string> Errors { get; } = new();
}

/// <summary>401 - credentials are missing, invalid or rejected.</summary>
public class AuthenticationFailedException : ApiException
{
    public AuthenticationFailedException(string message) : base(message, StatusCodes.Status401Unauthorized)
    {
    }
}

/// <summary>403 - the identity is known but not allowed to perform the action.</summary>
public class ForbiddenException : ApiException
{
    public ForbiddenException(string message) : base(message, StatusCodes.Status403Forbidden)
    {
    }
}

/// <summary>404 - the requested resource does not exist.</summary>
public class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound)
    {
    }
}

/// <summary>409 - the request conflicts with the current state, e.g. a duplicate email.</summary>
public class ConflictException : ApiException
{
    public ConflictException(string message) : base(message, StatusCodes.Status409Conflict)
    {
    }
}
