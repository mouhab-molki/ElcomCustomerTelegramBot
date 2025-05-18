namespace ElcomCustomerTelegramBot.MenusBuilder
{
    using Telegram.Bot;
    using Telegram.Bot.Types.ReplyMarkups;

    public static class MainMenu
    {
        public static async Task HideMenu(ITelegramBotClient bot, long chatId)
        {
            await bot.SendTextMessageAsync(chatId, " ", replyMarkup: new ReplyKeyboardRemove());
        }

        public static async Task ShowMainMenu(ITelegramBotClient bot, long chatId, string messageText)
        {
            var mainMenuKeyboard = new ReplyKeyboardMarkup(new[] {
                new[] { new KeyboardButton("عرض معلومات الاشتراك") },
                new[] { new KeyboardButton("عرض المعلومات المالية") },
                new[] { new KeyboardButton("عرض معلومات الباقة") },
                new[] { new KeyboardButton("تسجيل الخروج") }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendTextMessageAsync(chatId, "القائمة الرئيسية:", replyMarkup: mainMenuKeyboard);
        }
    }
}
