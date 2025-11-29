using LibraryManagement.WebAPI.Services.Interfaces;
using System.Security.Cryptography;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public void HashPassword(string password, out string hashedPassword, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(SaltSize);

        var key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        hashedPassword = Convert.ToBase64String(key.GetBytes(KeySize));
    }

    public bool VerifyPassword(string password, string hashedPassword, byte[] salt)
    {
        var key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var computed = Convert.ToBase64String(key.GetBytes(KeySize));

        return computed == hashedPassword;
    }
}
