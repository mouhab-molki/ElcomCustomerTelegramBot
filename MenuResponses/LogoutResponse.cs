namespace ElcomCustomerTelegramBot.MenuResponses
{
    using ElcomCustomerTelegramBot.Models;
    using Telegram.Bot;
    using Telegram.Bot.Types.ReplyMarkups;

    public static class LogoutResponse
    {
        public static async Task Logout(ITelegramBotClient bot, long chatId,
                                        Dictionary<long, string> userStates,
                                        Dictionary<long, string> phoneNumbers,
                                        Dictionary<long, string> nationalIds,
                                        Dictionary<long, string> tempNewPasswords,
                                        Dictionary<long, Subscriber> subscribers)
        {
            userStates.Remove(chatId);
            phoneNumbers.Remove(chatId);
            nationalIds.Remove(chatId);
            tempNewPasswords.Remove(chatId);
            subscribers.Remove(chatId);

            await bot.SendTextMessageAsync(chatId, "تم تسجيل الخروج بنجاح.", replyMarkup: new ReplyKeyboardRemove());
        }
    }
}
