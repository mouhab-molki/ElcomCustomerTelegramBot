namespace ElcomCustomerTelegramBot.MenusBuilder
{
    using System.Collections.Generic;
    using ElcomCustomerTelegramBot.Models;
    using Telegram.Bot;
    using Telegram.Bot.Types.ReplyMarkups;

    public static class FinancialMenu
    {
        public static async Task ShowFinancialInfoMenu(ITelegramBotClient bot, long chatId, string messageText, Dictionary<long, Subscriber> subscribers)
        {
            if (!subscribers.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, "لا يوجد مشترك بهذا المعرف.");
                return;
            }

            var subscriberData = subscribers[chatId];

            var subscriptionSubMenuKeyboard = new ReplyKeyboardMarkup(new[] {
                new[] { new KeyboardButton("عرض الرصيد المالي") },
                new[] { new KeyboardButton("عرض الرسوم الشهرية") },
                new[] { new KeyboardButton("عرض حالة التسديد وموعد الفاتورة القادمة") },
                new[] { new KeyboardButton("عرض سجل الدفعات") },
                new[] { new KeyboardButton("رجوع للقائمة الرئيسية") }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendTextMessageAsync(chatId, "قائمة المعلومات المالية:", replyMarkup: subscriptionSubMenuKeyboard);
        }
    }
}
