namespace Ecommerce.Api.Services;

/// <summary>
/// BCrypt based password hasher. BCrypt salts every hash automatically and is deliberately slow,
/// which is what makes stored hashes resistant to offline cracking.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Work factor. 12 is a sensible balance between login latency and resistance to brute force.
    /// Stored hashes carry their own cost factor, so raising this later does not invalidate
    /// passwords that were hashed with an older value.
    /// </summary>
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // The stored value is not a BCrypt hash (for example legacy data). It cannot be verified.
            return false;
        }
    }
}
