using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using ElcomCustomerTelegramBot.Models;

namespace ElcomCustomerTelegramBot.MenuServices
{
    public static class GeneralInfoMenuService
    {
        public static async Task ShowGeneralInfoMenu(ITelegramBotClient bot, long chatId, string messageText, Dictionary<long, Subscriber> subscribers)
        {
            var subscriberData = subscribers[chatId];


            var generalInfoKeyboard = new ReplyKeyboardMarkup(new[] {
        new[] { new KeyboardButton("عرض الشريحة الحالية") },
        new[] { new KeyboardButton("عرض حالة الاشتراك") },
        new[] { new KeyboardButton("رجوع لقائمة معلومات الاشتراك") }
    })
            {
                ResizeKeyboard = true
            };

            await bot.SendTextMessageAsync(chatId, "قائمة المعلومات العامة:", replyMarkup: generalInfoKeyboard);
        }
    }
}
