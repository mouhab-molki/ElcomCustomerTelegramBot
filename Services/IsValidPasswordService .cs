using System.Security.Cryptography;
using System.Text;

namespace ElcomCustomerTelegramBot.Services
{
    public static class IsValidPasswordService
    {
        public static bool IsValidPassword(string password)
        {
            if (password.Length < 8)
                return false;

            bool hasLetter = password.Any(char.IsLetter) && password.All(c => c < 128);
            bool hasDigit = password.Any(char.IsDigit);

            return hasLetter && hasDigit;
        }
    }
}