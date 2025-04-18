﻿namespace ElcomCustomerTelegramBot.MenuResponses
{
    using System.Text.Json;
    using Telegram.Bot;
    using Telegram.Bot.Types.Enums;
    using ElcomCustomerTelegramBot.Models;

    public static class MonthlyPriceResponse
    {
        public static async Task ShowMonthlyPrice(ITelegramBotClient bot, long chatId, Dictionary<long, Subscriber> subscribers, string apiBaseUrl)
        {
            if (!subscribers.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, "❌ لم يتم العثور على بيانات المشترك.");
                return;
            }

            var subscriber = subscribers[chatId];

            try
            {
                using var http = new HttpClient();

                var response = await http.GetAsync($"{apiBaseUrl}/api/Packages/{subscriber.PackageId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // مهمة إذا أسماء الـ JSON ما بتطابق C# case
                    };

                    var packageInfo = JsonSerializer.Deserialize<PackageResponse>(json, options);

                    if (packageInfo != null)
                    {
                        string reply = $"*الشريحة:* {packageInfo.PackageName}\n" +
                                       $"*السعر:* {packageInfo.MonthlyPrice} ل.س";



                        await bot.SendTextMessageAsync(chatId, reply, ParseMode.Markdown);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(chatId, "❌ تعذر قراءة معلومات الباقة من الخادم.");
                    }
                }
            }
            catch (Exception ex)
            {
                await bot.SendTextMessageAsync(chatId, $"❌ حدث خطأ أثناء الاتصال بالخادم: {ex.Message}");
            }
        }
    }
}
