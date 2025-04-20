using Telegram.Bot;
using ElcomCustomerTelegramBot.Models;

namespace ElcomCustomerTelegramBot.MenuResponses
{
    public static class SubscriberBalancePackageResponse
    {
        // عرض حالة الباقة
        public static async Task ShowPackageBalanceAsync(ITelegramBotClient bot, long chatId, string apiBaseUrl, string phoneNumber)
        {
            // جلب البيانات المحدثة من الـ API
            var updatedSubscriberData = await Services.SubscriberDataService.GetSubscriberData(apiBaseUrl, phoneNumber);

            if (updatedSubscriberData == null)
            {
                await bot.SendTextMessageAsync(chatId, "❌ تعذر تحديث بيانات الباقة. يرجى المحاولة لاحقًا.");
                return;
            }

            // إرسال الرسالة بناءً على البيانات المحدثة
            await bot.SendTextMessageAsync(
                chatId,
                $"متبقي من باقة حضرتك: {updatedSubscriberData.DownloadMonthlyBalance} GB\n" +
                $"حجم الباقة الإجمالي لهذا الشهر: {updatedSubscriberData.TotalDownloadMonthlyBalance} GB"
            );
        }

        // عرض الرصيد الحالي
        public static async Task ShowCurrentBalanceAsync(ITelegramBotClient bot, long chatId, string apiBaseUrl, string phoneNumber)
        {
            // جلب البيانات المحدثة من الـ API
            var updatedSubscriberData = await Services.SubscriberDataService.GetSubscriberData(apiBaseUrl, phoneNumber);

            if (updatedSubscriberData == null)
            {
                await bot.SendTextMessageAsync(chatId, "❌ تعذر تحديث بيانات الرصيد. يرجى المحاولة لاحقًا.");
                return;
            }

            // إرسال الرسالة بناءً على البيانات المحدثة
            await bot.SendTextMessageAsync(
                chatId,
                $"رصيدك الحالي هو: {updatedSubscriberData.Balance} ل.س"
            );
        }
    }
}
