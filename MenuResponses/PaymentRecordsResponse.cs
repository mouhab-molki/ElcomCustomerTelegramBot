﻿namespace ElcomCustomerTelegramBot.MenuResponses
{
    using System.Text.Json;
    using System.Text;
    using ElcomCustomerTelegramBot.Models;
    using Telegram.Bot;
    using Telegram.Bot.Types.ReplyMarkups;

    public static class PaymentRecordsResponse
    {
        public static async Task DisplayPaymentRecords(ITelegramBotClient bot, long chatId, int currentPage, Dictionary<long, Subscriber> subscribers, string apiBaseUrl)
        {
            if (!subscribers.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, "❌ لم يتم العثور على بيانات المشترك.");
                return;
            }

            var sub = subscribers[chatId];

            try
            {
                string requestUrl = $"{apiBaseUrl}/api/UserFinancialStatement/GetUserFinancialStatement/{sub.SubscriberId}";

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var paymentRecords = JsonSerializer.Deserialize<List<PaymentRecord>>(content);

                if (paymentRecords == null || !paymentRecords.Any())
                {
                    await bot.SendTextMessageAsync(chatId, "لا توجد سجلات مالية.");
                    return;
                }

                int recordsPerPage = 3;
                int totalRecords = paymentRecords.Count;
                int totalPages = (int)Math.Ceiling(totalRecords / (double)recordsPerPage);

                currentPage = Math.Clamp(currentPage, 1, totalPages);

                int startIndex = (currentPage - 1) * recordsPerPage;
                int endIndex = Math.Min(startIndex + recordsPerPage, totalRecords);

                StringBuilder messageBuilder = new StringBuilder();
                for (int i = startIndex; i < endIndex; i++)
                {
                    var paymentsrecord = paymentRecords[i];
                    messageBuilder.AppendLine($"قيمة الدفعة: {paymentsrecord.PaymentAmount}");
                    if (paymentsrecord.AdditionDate.HasValue)
                        messageBuilder.AppendLine($"تاريخ الإضافة: {paymentsrecord.AdditionDate.Value:yyyy-MM-dd HH:mm:ss}");
                    else if (paymentsrecord.WithdrawalDate.HasValue)
                        messageBuilder.AppendLine($"تاريخ السحب: {paymentsrecord.WithdrawalDate.Value:yyyy-MM-dd HH:mm:ss}");
                    messageBuilder.AppendLine("--------------------");
                }

                InlineKeyboardButton previousButton = currentPage > 1
                    ? InlineKeyboardButton.WithCallbackData("السابق", $"paymentPage_{currentPage - 1}")
                    : InlineKeyboardButton.WithCallbackData(" ", "noop");

                InlineKeyboardButton nextButton = currentPage < totalPages
                    ? InlineKeyboardButton.WithCallbackData("التالي", $"paymentPage_{currentPage + 1}")
                    : InlineKeyboardButton.WithCallbackData(" ", "noop");

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] { previousButton, nextButton }
                });

                await bot.SendTextMessageAsync(chatId, messageBuilder.ToString(), replyMarkup: inlineKeyboard);
            }
            catch (Exception ex)
            {
                await bot.SendTextMessageAsync(chatId, $"❌ حدث خطأ: {ex.Message}");
            }
        }
    }
}
