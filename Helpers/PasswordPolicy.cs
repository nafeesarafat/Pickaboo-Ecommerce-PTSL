namespace Ecommerce.Api.Helpers;

/// <summary>
/// Single place where the password policy is defined, so registration and password change
/// enforce exactly the same rules.
/// </summary>
public static class PasswordPolicy
{
    public const int MinimumLength = 8;
    public const int MaximumLength = 100;

    /// <summary>
    /// Returns one message per rule the password breaks. An empty list means the password is valid.
    /// </summary>
    public static List<string> Validate(string? password)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            failures.Add("Password is required.");
            return failures;
        }

        if (password.Length < MinimumLength)
        {
            failures.Add($"Password must be at least {MinimumLength} characters long.");
        }

        // BCrypt only considers the first 72 bytes, so longer input is rejected instead of silently truncated.
        if (password.Length > MaximumLength)
        {
            failures.Add($"Password must be at most {MaximumLength} characters long.");
        }

        if (!password.Any(char.IsUpper))
        {
            failures.Add("Password must contain at least one uppercase letter.");
        }

        if (!password.Any(char.IsLower))
        {
            failures.Add("Password must contain at least one lowercase letter.");
        }

        if (!password.Any(char.IsDigit))
        {
            failures.Add("Password must contain at least one digit.");
        }

        // A "special" character is anything that is not a letter, a digit or whitespace.
        // char.IsSymbol alone is not enough: it excludes common punctuation such as '!' and '@'.
        if (!password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
        {
            failures.Add("Password must contain at least one special character.");
        }

        return failures;
    }

    public static bool IsValid(string? password) => Validate(password).Count == 0;
}
