using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using ElcomCustomerTelegramBot.Models;

namespace ElcomCustomerTelegramBot.MenuServices
{
    public static class PackageMenuService
    {
        public static async Task ShowPackageInfoMenu(ITelegramBotClient bot, long chatId, string messageText, Dictionary<long, Subscriber> subscribers)
        {
            var subscriberData = subscribers[chatId];


            var subscriptionSubMenuKeyboard = new ReplyKeyboardMarkup(new[] {
        new[] { new KeyboardButton("التحقق من الباقة") },
        new[] { new KeyboardButton("شراء حجوم إضافية") },
        new[] { new KeyboardButton("عرض سجل شراء الباقات") },
        new[] { new KeyboardButton("رجوع للقائمة الرئيسية") }
    })
            {
                ResizeKeyboard = true
            };

            await bot.SendTextMessageAsync(chatId, "قائمة معلومات الباقة:", replyMarkup: subscriptionSubMenuKeyboard);
        }
    }
}
