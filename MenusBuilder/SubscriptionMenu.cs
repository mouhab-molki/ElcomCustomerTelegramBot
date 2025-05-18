namespace ElcomCustomerTelegramBot.MenusBuilder
{
    using System.Collections.Generic;
    using ElcomCustomerTelegramBot.Models;
    using Telegram.Bot;
    using Telegram.Bot.Types.ReplyMarkups;

    public static class SubscriptionMenu
    {
        public static async Task ShowSubscriptionInfoMenu(ITelegramBotClient bot, long chatId, string messageText, Dictionary<long, Subscriber> subscribers)
        {
            if (!subscribers.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, "لا يوجد مشترك بهذا المعرف.");
                return;
            }

            var subscriberData = subscribers[chatId];

            var subscriptionSubMenuKeyboard = new ReplyKeyboardMarkup(new[] {
                new[] { new KeyboardButton("عرض اسم المستخدم وكلمة المرور") },
                new[] { new KeyboardButton("عرض المعلومات العامة") },
                new[] { new KeyboardButton("معلومات الاتصال") },
                new[] { new KeyboardButton("رجوع للقائمة الرئيسية") }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendTextMessageAsync(chatId, "قائمة معلومات الاشتراك:", replyMarkup: subscriptionSubMenuKeyboard);
        }
    }
}
