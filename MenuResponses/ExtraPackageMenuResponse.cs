namespace ElcomCustomerTelegramBot.MenuResponses
{
    using System.Text.Json;
    using System.Text;
    using ElcomCustomerTelegramBot.Models;
    using Telegram.Bot;
    using Telegram.Bot.Types.ReplyMarkups;
    using ElcomCustomerTelegramBot.Services;

    public static class ExtraPackageMenuResponse
    {
        public static async Task DisplayExtraPackagePurchaseRecords(ITelegramBotClient bot, long chatId, int currentPage, Dictionary<long, Subscriber> subscribers, string apiBaseUrl)
        {
            if (!subscribers.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, "❌ لم يتم العثور على بيانات المشترك.");
                return;
            }

            var sub = subscribers[chatId];

            try
            {
                string requestUrl = $"{apiBaseUrl}/api/ExtraPackagePurchases/subscriber/{sub.SubscriberId}";
                using var client = new HttpClient();
                var response = await client.GetAsync(requestUrl);


                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await bot.SendTextMessageAsync(chatId, "لا توجد سجلات لشراء الباقات.");
                    return;
                }
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var purchaseRecords = JsonSerializer.Deserialize<List<ExtraPackagePurchase>>(content);
 

                int recordsPerPage = 3;
                int totalRecords = purchaseRecords.Count;
                int totalPages = (int)Math.Ceiling(totalRecords / (double)recordsPerPage);

                currentPage = Math.Clamp(currentPage, 1, totalPages);

                int startIndex = (currentPage - 1) * recordsPerPage;
                int endIndex = Math.Min(startIndex + recordsPerPage, totalRecords);

                StringBuilder messageBuilder = new StringBuilder();

                for (int i = startIndex; i < endIndex; i++)
                {
                    var record = purchaseRecords[i];
                    messageBuilder.AppendLine($"الباقة الإضافية: {record.Notes}");
                    messageBuilder.AppendLine($"السعر: {record.ChargedAmount}");
                    messageBuilder.AppendLine($"تاريخ الشراء: {record.PurchaseDate:yyyy-MM-dd HH:mm:ss}");
                    messageBuilder.AppendLine($"حالة التفعيل: {record.ActivationStatus}");
                    messageBuilder.AppendLine($"نوع العملية: {record.PurchaseType}");
                    messageBuilder.AppendLine("--------------------");
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    currentPage > 1 ? InlineKeyboardButton.WithCallbackData("السابق", $"extraPage_{currentPage - 1}") : InlineKeyboardButton.WithCallbackData(" ", "noop"),
                    currentPage < totalPages ? InlineKeyboardButton.WithCallbackData("التالي", $"extraPage_{currentPage + 1}") : InlineKeyboardButton.WithCallbackData(" ", "noop")
                });

                await bot.SendTextMessageAsync(chatId, messageBuilder.ToString(), replyMarkup: inlineKeyboard);
            }
            catch (Exception ex)
            {

                await bot.SendTextMessageAsync(chatId, $"❌ حدث خطأ: {ex.Message}");
            }
        }

        public static async Task DisplayExtraPackagesAsync(ITelegramBotClient bot, long chatId, string apiBaseUrl, CancellationToken cancellationToken)
        {
            using var http = new HttpClient();
            string url = $"{apiBaseUrl}/api/ExtraPackages";
            var response = await http.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await bot.SendTextMessageAsync(chatId, "حدث خطأ أثناء تحميل الباقات الإضافية ❌", cancellationToken: cancellationToken);
                return;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var packages = JsonSerializer.Deserialize<List<ExtraPackage>>(content);

            if (packages == null || !packages.Any())
            {
                await bot.SendTextMessageAsync(chatId, "لا توجد باقات إضافية حالياً 📭", cancellationToken: cancellationToken);
                return;
            }

            var buttons = packages.Select(p =>
                InlineKeyboardButton.WithCallbackData($"باقة {p.SizeGb}GB بسعر {p.Price} ل.س", $"selectExtra_{p.ExtraPackageId}")
            ).ToList();

            var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }));

            await bot.SendTextMessageAsync(chatId, "اختر إحدى الباقات الإضافية المتوفرة:", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        public static async Task HandleExtraPackageSelectionAsync(ITelegramBotClient bot, long chatId, int extraPackageId, string apiBaseUrl, CancellationToken cancellationToken)
        {
            using var http = new HttpClient();
            var response = await http.GetAsync($"{apiBaseUrl}/api/ExtraPackages", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var packages = JsonSerializer.Deserialize<List<ExtraPackage>>(content);

            var selectedPackage = packages?.FirstOrDefault(p => p.ExtraPackageId == extraPackageId);
            if (selectedPackage == null)
            {
                await bot.SendTextMessageAsync(chatId, "الباقة غير موجودة ❌", cancellationToken: cancellationToken);
                return;
            }

            var confirmButtons = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ تأكيد", $"confirmExtra_{extraPackageId}"),
                    InlineKeyboardButton.WithCallbackData("❌ إلغاء", "cancelExtra")
                }
            });

            await bot.SendTextMessageAsync(chatId,
                $"هل أنت متأكد من شراء باقة {selectedPackage.SizeGb}GB بسعر {selectedPackage.Price} ل.س؟",
                replyMarkup: confirmButtons,
                cancellationToken: cancellationToken);
        }

        public static async Task ConfirmExtraPackagePurchaseAsync(ITelegramBotClient bot, long chatId, int extraPackageId, Dictionary<long, Subscriber> subscribers, string apiBaseUrl, CancellationToken cancellationToken)
        {
            if (!subscribers.TryGetValue(chatId, out var subscriber))
            {
                await bot.SendTextMessageAsync(chatId, "يجب تسجيل الدخول أولاً 🔐", cancellationToken: cancellationToken);
                return;
            }

            using var http = new HttpClient();
            var response = await http.GetAsync($"{apiBaseUrl}/api/ExtraPackages", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var packages = JsonSerializer.Deserialize<List<ExtraPackage>>(content);
            var selectedPackage = packages?.FirstOrDefault(p => p.ExtraPackageId == extraPackageId);

            if (selectedPackage == null)
            {
                await bot.SendTextMessageAsync(chatId, "الباقة غير موجودة ❌", cancellationToken: cancellationToken);
                return;
            }

            if (subscriber.Balance < selectedPackage.Price)
            {
                await bot.SendTextMessageAsync(chatId, $"رصيدك غير كافٍ لشراء هذه الباقة 💸\nرصيدك الحالي: {subscriber.Balance} ل.س", cancellationToken: cancellationToken);
                return;
            }

            var postUrl = $"{apiBaseUrl}/api/ExtraPackagePurchases/{subscriber.SubscriberId}/ExtraPackagePurchases/{extraPackageId}";
            var postResult = await http.PostAsync(postUrl, null, cancellationToken);

            if (postResult.IsSuccessStatusCode)
            {
                await bot.SendTextMessageAsync(chatId, $"✅ تم شراء باقة {selectedPackage.SizeGb}GB بسعر {selectedPackage.Price} ل.س بنجاح", cancellationToken: cancellationToken);
                var subscriberData = await SubscriberDataService.GetSubscriberData(apiBaseUrl, subscriber.PhoneNumber);
            }
            else
            {
                await bot.SendTextMessageAsync(chatId, $"❌ فشل شراء باقة {selectedPackage.SizeGb}GB بسعر {selectedPackage.Price} ل.س", cancellationToken: cancellationToken);
            }
        }

        public static async Task CancelExtraPackagePurchaseAsync(ITelegramBotClient bot, long chatId, CancellationToken cancellationToken)
        {
            await DisplayExtraPackagesAsync(bot, chatId, null, cancellationToken);
        }
    }
}
