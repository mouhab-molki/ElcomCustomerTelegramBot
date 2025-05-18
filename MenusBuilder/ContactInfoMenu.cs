using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using ElcomCustomerTelegramBot.Models;
using ElcomCustomerTelegramBot.Services;

namespace ElcomCustomerTelegramBot.MenusBuilder
{
    public static class ContactInfoMenu
    {
        public static async Task ShowContactInfoMenu(ITelegramBotClient bot, long chatId, string messageText, Dictionary<long, Subscriber> subscribers)
        {
           

            var subscriberData = subscribers[chatId];

            var subscriptionSubMenuKeyboard = new ReplyKeyboardMarkup(new[] {
                new[] { new KeyboardButton("عرض معلومات الإتصال") },
                new[] { new KeyboardButton("تغيير كلمة سر واجهة المشتركين") },
                new[] { new KeyboardButton("رجوع لقائمة معلومات الاشتراك") }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendTextMessageAsync(chatId, "قائمة تعديل معلومات الاتصال", replyMarkup: subscriptionSubMenuKeyboard);
        }
    }
}
