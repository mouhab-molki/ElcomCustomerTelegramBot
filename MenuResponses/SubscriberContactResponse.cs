namespace ElcomCustomerTelegramBot.MenuResponses
{
    using System.Collections.Generic;
    using Telegram.Bot;
    using Telegram.Bot.Types.Enums;
    using ElcomCustomerTelegramBot.Models;

    public static class SubscriberContactResponse
    {
        public static async Task ShowContactInfo(ITelegramBotClient bot, long chatId, Dictionary<long, Subscriber> subscribers)
        {
            if (!subscribers.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, "❌ لم يتم العثور على بيانات المشترك.");
                return;
            }

            var subscriberData = subscribers[chatId];
            string contactInfo = $"اسم المشترك: {subscriberData.FullName}\n" +
                                 $"الرقم الأرضي: {subscriberData.PhoneNumber}\n" +
                                 $"رقم الموبايل: {subscriberData.Mobile}\n" +
                                 $"الإيميل: {subscriberData.Email}\n" +
                                 $"العنوان: {subscriberData.Address}\n" +
                                 $"الشركة: الشركة السورية للتكنولوجيا";

            await bot.SendTextMessageAsync(chatId, contactInfo, parseMode: ParseMode.Markdown);
        }
    }
}
