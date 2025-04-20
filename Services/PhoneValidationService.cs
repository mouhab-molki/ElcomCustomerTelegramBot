using System.Text;
using Telegram.Bot;
namespace ElcomCustomerTelegramBot.Services
{
    public static class PhoneValidationService
    {
        public static (bool isValid, string errorMessage, string cleanedPhone) ValidatePhonePrefix(string phoneNumber)
        {
            string cleanedPhone = phoneNumber.Replace("-", "").Trim();
            if (string.IsNullOrWhiteSpace(cleanedPhone))
                return (false, "يرجى كتابة الرقم الأرضي مع نداء المحافظة 🥱!", cleanedPhone);

            if (!cleanedPhone.All(char.IsDigit))
                return (false, "رقم الهاتف يجب أن يحتوي فقط على أرقام 🤣", cleanedPhone);

            var allowedPrefixes = new[] { "011", "021", "031", "033", "041", "043", "023", "051", "052", "022", "016", "015", "014" };
            string prefix = allowedPrefixes.FirstOrDefault(p => cleanedPhone.StartsWith(p));
            if (prefix == null)
                return (false, "الرقم يجب أن يضم نداء المحافظة 😏.", cleanedPhone);

            int expectedLength = prefix == "016" ? 9 : 10;
            if (cleanedPhone.Length != expectedLength)
                return (false, $"عدد أرقام الهاتف يجب أن يكون {expectedLength} رقماً 💁🏻‍♂️.", cleanedPhone);

            return (true, string.Empty, cleanedPhone);
        }
    }
}
