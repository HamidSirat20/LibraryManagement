using LibraryManagement.WebAPI.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class PasswordService : IPasswordService
{
    public void HashPassword(string originalPassword, out string hashedPassword, out byte[] salt)
    {
        var hcam = new HMACSHA256();
        salt = RandomNumberGenerator.GetBytes(16);
        hashedPassword = Encoding.UTF8.GetString(
            hcam.ComputeHash(Encoding.UTF8.GetBytes(originalPassword))
        );
    }

    public bool VerifyPassword(string originalPassword, string hashedPassword, byte[] salt)
    {
        var hcam = new HMACSHA256(salt);
        var hashedOriginal = Encoding.UTF8.GetString(
            hcam.ComputeHash(Encoding.UTF8.GetBytes(originalPassword))
        );
        return hashedOriginal == hashedOriginal;
    }
}
