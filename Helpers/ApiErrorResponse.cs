namespace Ecommerce.Api.Helpers;

/// <summary>
/// Single error shape used by every API response. Internal exception details are never
/// placed in here for production responses.
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static ApiErrorResponse Create(string message, IEnumerable<string>? errors = null)
    {
        return new ApiErrorResponse
        {
            Success = false,
            Message = message,
            Errors = errors?.ToList() ?? new List<string>()
        };
    }
}
