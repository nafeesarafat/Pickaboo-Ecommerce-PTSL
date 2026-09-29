namespace Ecommerce.Api.Services;

/// <summary>
/// Password hashing abstraction. Implementations must never store or return plain text passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hashes a plain password, generating a fresh salt. The result is what gets stored.</summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a plain password against a stored hash.
    /// Returns false for null/empty input instead of throwing.
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
