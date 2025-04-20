namespace ElcomCustomerTelegramBot.MenuResponses
{
    using System.Text.Json;
    using ElcomCustomerTelegramBot.Models;
    using Telegram.Bot;
    using Telegram.Bot.Types.Enums;

    public  class InvoiceMenuResponse
    {
        public async Task ShowInvoiceStatusAsync(ITelegramBotClient bot, long chatId, Dictionary<long, Subscriber> subscribers, string apiBaseUrl)
        {
            if (!subscribers.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, "❌ لم يتم العثور على بيانات المشترك.");
                return;
            }

            var sub = subscribers[chatId];

            string apiUrl = $"{apiBaseUrl}/api/Invoices/subscriber/{sub.SubscriberId}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var invoices = JsonSerializer.Deserialize<List<InvoiceResponse>>(json);

                if (invoices != null && invoices.Any())
                {
                    var latestInvoice = invoices.OrderByDescending(i => i.IssueDate).First();
                    string paymentStatus = latestInvoice.IsPaid ? "رسومكم عن الشهر الحالي مسددة" : "رسومكم عن الشهر الحالي غير مسددة";

                    DateTime nextInvoiceDate = sub.SubscriptionStatus == "غير مفعل"
                        ? latestInvoice.IssueDate
                        : new DateTime(latestInvoice.IssueDate.AddMonths(1).Year, latestInvoice.IssueDate.AddMonths(1).Month, 1);

                    string invoiceMessage = $"حالة التسديد: {paymentStatus}\n" +
                                            $"موعد الفاتورة القادمة هي: {nextInvoiceDate:yyyy-MM-dd}";

                    await bot.SendTextMessageAsync(chatId, invoiceMessage, ParseMode.Markdown);
                }
                else
                {
                    await bot.SendTextMessageAsync(chatId, "❌ لا توجد فواتير مسجلة لهذا المشترك.");
                }
            }
            else
            {
                await bot.SendTextMessageAsync(chatId, "❌ فشل في جلب بيانات الفواتير.");
            }
        }
    }
}
