using System.Security.Cryptography;
using System.Text;

namespace ElcomCustomerTelegramBot.Services
{
    public static class PasswordHashingService
    {
        public static string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            byte[] bytes = Encoding.UTF8.GetBytes(saltedPassword);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}