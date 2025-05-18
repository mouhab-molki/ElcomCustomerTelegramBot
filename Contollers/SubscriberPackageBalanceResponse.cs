using Telegram.Bot;
using ElcomCustomerTelegramBot.Models;

namespace ElcomCustomerTelegramBot.Controllers
{
    public static class SubscriberPackageBalanceResponse
    {

        //Show Package Balance
        public static async Task ShowPackageBalance(ITelegramBotClient bot, long chatId, string apiBaseUrl, string phoneNumber)
        {
            // Get updated subscriber information 
            var updatedSubscriberData = await Services.SubscriberDataService.GetSubscriberData(apiBaseUrl, phoneNumber);

            if (updatedSubscriberData == null)
            {
                await bot.SendTextMessageAsync(chatId, "❌ حدث خطأ ما, يرجى إعادة تسجيل الدخول أو إعادة المحاولة لاحقاً.");
                return;
            }

            await bot.SendTextMessageAsync(
                chatId,
                $"تم استهلاك من باقة حضرتك: {updatedSubscriberData.DownloadMonthlyBalance} GB\n" +
                $"حجم الباقة الأساسي: {updatedSubscriberData.MonthlyDataVolumeGb} GB\n" +
                $"حجم الباقة الإجمالي لهذا الشهر: {updatedSubscriberData.TotalDownloadMonthlyBalance} GB"
            );
        }

        //Show Subscriber Balance
        public static async Task ShowCurrentBalance(ITelegramBotClient bot, long chatId, string apiBaseUrl, string phoneNumber)
        {
            // Get updated subscriber information 
            var updatedSubscriberData = await Services.SubscriberDataService.GetSubscriberData(apiBaseUrl, phoneNumber);

            if (updatedSubscriberData == null)
            {
                await bot.SendTextMessageAsync(chatId, "❌ تعذر تحديث بيانات الرصيد. يرجى المحاولة لاحقًا.");
                return;
            }

            await bot.SendTextMessageAsync(
                chatId,
                $"رصيدك الحالي هو: {updatedSubscriberData.Balance} ل.س"
            );
        }
    }
}
