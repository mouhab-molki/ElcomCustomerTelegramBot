using ElcomCustomerTelegramBot.Models;
using System.Text.Json;

namespace ElcomCustomerTelegramBot.Services
{

    public static class SubscriberDataService
    {
        public static async Task<Subscriber?> GetSubscriberData(string apiBaseUrl, string phoneNumber)
        {
            try
            {
                using var http = new HttpClient();
                string url = $"{apiBaseUrl}/api/Subscribers/phone/{phoneNumber}";
                var response = await http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Subscriber>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }

       
    }
}