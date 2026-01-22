using Konta.Identity.Services.Interfaces;

namespace Konta.Identity.Services.Implementations;

/// <summary>
/// Implémentation du service de hachage de mot de passe utilisant BCrypt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc />
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    /// <inheritdoc />
    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception)
        {
            // Retourne false si le hash est malformé au lieu de crasher le service
            return false;
        }
    }
}
