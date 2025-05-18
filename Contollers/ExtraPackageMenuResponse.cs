namespace ElcomCustomerTelegramBot.Controllers
{
    using System.Text.Json;
    using System.Text;
    using ElcomCustomerTelegramBot.Models;
    using Telegram.Bot;
    using Telegram.Bot.Types.ReplyMarkups;
    using ElcomCustomerTelegramBot.Services;

    public static class ExtraPackageMenuResponse
    {
        public static async Task DisplayExtraPackagePurchaseHistory(ITelegramBotClient bot, long chatId, int currentPage, Dictionary<long, Subscriber> subscribers, string apiBaseUrl)
        {
            if (!subscribers.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, "❌ حدث خطأ ما, يرجى إعادة تسجيل الدخول أو إعادة المحاولة لاحقاً.");
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
                    await bot.SendTextMessageAsync(chatId, "لا يوجد باقات تم شراؤها سابقاً، سيتم عرض الباقات التي تم شراؤها عند شرائك يرجى إعادة المحاولة لاحقاً باقات في المستقبل.");
                    return;
                }
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                var purchaseRecords = JsonSerializer.Deserialize<List<ExtraPackagePurchase>>(content);

                //ٍSet the number or extra package purchase history by page and the pagging functionality
                int recordsPerPage = 3;
                int totalRecords = (purchaseRecords == null || !purchaseRecords.Any()) ? 1 : purchaseRecords.Count;
                int totalPages = (int)Math.Ceiling(totalRecords / (double)recordsPerPage);

                currentPage = Math.Clamp(currentPage, 1, totalPages);

                //To set which purchase histroy records are shown in the message
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
                    currentPage > 1 ? InlineKeyboardButton.WithCallbackData("السابق", $"PackagePurchaseHistory_{currentPage - 1}") : InlineKeyboardButton.WithCallbackData(" ", "ignore"),
                    currentPage < totalPages ? InlineKeyboardButton.WithCallbackData("التالي", $"PackagePurchaseHistory_{currentPage + 1}") : InlineKeyboardButton.WithCallbackData(" ", "ignore")
                });

                await bot.SendTextMessageAsync(chatId, messageBuilder.ToString(), replyMarkup: inlineKeyboard);
            }
            catch (Exception ex)
            {

                await bot.SendTextMessageAsync(chatId, $"❌ حدث خطأ: {ex.Message}");
            }
        }

        //Buy Extra Package Section

        //Display the extra packges withe theire prices 
        public static async Task DisplayExtraPackages(ITelegramBotClient bot, long chatId, string apiBaseUrl, CancellationToken cancellationToken)
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
                InlineKeyboardButton.WithCallbackData($"باقة {p.SizeGb}GB بسعر {p.Price} ل.س", $"SelectExtraPackagePurchace_{p.ExtraPackageId}")
            ).ToList();

            var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }));

            await bot.SendTextMessageAsync(chatId, "اختر إحدى الباقات الإضافية المتوفرة:", replyMarkup: keyboard, cancellationToken: cancellationToken);
        }

        // show the selected the extra Package info
        public static async Task ExtraPackageSelection(ITelegramBotClient bot, long chatId, int extraPackageId, string apiBaseUrl, CancellationToken cancellationToken)
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
                    InlineKeyboardButton.WithCallbackData("✅ تأكيد", $"ConfirmExtraPackagePurchace_{extraPackageId}"),
                    InlineKeyboardButton.WithCallbackData("❌ إلغاء", "CancelExtraPackagePurchace_")
                }
            });

            await bot.SendTextMessageAsync(chatId,
                $"هل أنت متأك يرجى إعادة المحاولة لاحقاًد من شراء باقة {selectedPackage.SizeGb}GB بسعر {selectedPackage.Price} ل.س؟",
                replyMarkup: confirmButtons,
                cancellationToken: cancellationToken);
        }

        // Buy the extra package functionalityy
        public static async Task ConfirmExtraPackagePurchase(ITelegramBotClient bot, long chatId, int extraPackageId, Dictionary<long, Subscriber> subscribers, string apiBaseUrl, CancellationToken cancellationToken)
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

            if (subscriber.SubscriptionStatus == "غير مفعل")
            {
                await bot.SendTextMessageAsync(chatId, " اشتراكك غير مفعل, يرجى تسديد الرسوم الشهرية المترتبة عليك قبل شراء حجوم إضافية 🤨", cancellationToken: cancellationToken);
                return;
               

            }


            if (subscriber.Balance < selectedPackage.Price)
            {
                await bot.SendTextMessageAsync(chatId, $"رصيدك غير كافٍ لشراء هذه الباقة 💸\nرصيدك الحالي: {subscriber.Balance} ل.س\nيرجى إعادة المحاولة لاحقاً بعد تسديد مبلغ {selectedPackage.Price - subscriber.Balance} ل.س لإتمام عملية الشراء.", cancellationToken: cancellationToken);
                return;
            }

            var postUrl = $"{apiBaseUrl}/api/ExtraPackagePurchases/{subscriber.SubscriberId}/ExtraPackagePurchases/{extraPackageId}";
    
            var json = JsonSerializer.Serialize("شراء عبر بوت التلغرام");
            var Postcontent = new StringContent(json, Encoding.UTF8, "application/json");
            var postResult = await http.PostAsync(postUrl, Postcontent);
            Console.WriteLine(postResult);

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

        public static async Task CancelExtraPackagePurchase(ITelegramBotClient bot, long chatId, string apiBaseUrl, CancellationToken cancellationToken)
        {
            await DisplayExtraPackages(bot, chatId, apiBaseUrl, cancellationToken);
        }
    }
}
