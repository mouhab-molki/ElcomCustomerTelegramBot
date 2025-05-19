using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups; 
using System.Text;
using System.Text.Json; 

//Import the controllers and the Data Models and the Menus Builder
using ElcomCustomerTelegramBot.Services;
using ElcomCustomerTelegramBot.Models;
using ElcomCustomerTelegramBot.MenusBuilder;
using ElcomCustomerTelegramBot.Controllers;
using System.Threading;


var botClient = new TelegramBotClient("xxxxx");
var apiBaseUrl = "xxxxx"

// Stores user-related data for tracking state, authentication, and subscription details.
var userStates = new Dictionary<long, string>();
var phoneNumbers = new Dictionary<long, string>();
var nationalIds = new Dictionary<long, string>();
var subscribers = new Dictionary<long, Subscriber>();
var tempNewPasswords = new Dictionary<long, string>();

using var InitializeCancellationToken = new CancellationTokenSource();
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    HandleUpdates,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: InitializeCancellationToken.Token
);




Console.WriteLine("Bot is running. Press Enter to exit.");
Console.ReadLine();

async Task HandleUpdates(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
{
    // Handling in message button clicks
    if (update.Type == UpdateType.CallbackQuery)
    {
        // Extracting in message button callback query and retrieving chat ID for handling user interactions
        var callbackQuery = update.CallbackQuery!;
        var callbackChatId = callbackQuery.Message!.Chat.Id;

        //Handle Extra Packages Purchase in message Butons
        if (callbackQuery.Data!.StartsWith("SelectExtraPackagePurchace_"))
        {
            int extraPackageId = int.Parse(callbackQuery.Data.Replace("SelectExtraPackagePurchace_", ""));
            await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken);
            await ExtraPackageMenuResponse.ExtraPackageSelection(bot, callbackChatId, extraPackageId, apiBaseUrl, cancellationToken);
            return;
        }
        else if (callbackQuery.Data!.StartsWith("ConfirmExtraPackagePurchace_"))
        {
            int extraPackageId = int.Parse(callbackQuery.Data.Replace("ConfirmExtraPackagePurchace_", ""));
            await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken);
            await ExtraPackageMenuResponse.ConfirmExtraPackagePurchase(bot, callbackChatId, extraPackageId, subscribers, apiBaseUrl, cancellationToken);
            return;
        }
        else if (callbackQuery.Data == "CancelExtraPackagePurchace_")
        {
            await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken);
            await ExtraPackageMenuResponse.CancelExtraPackagePurchase(bot, callbackChatId, apiBaseUrl, cancellationToken);
            return;
        }

        //Handle Payments in message Butons
        if (callbackQuery.Data!.StartsWith("paymentPage_"))
        {
            int requestedPage = int.Parse(callbackQuery.Data.Replace("paymentPage_", ""));
             
            try
            {
                await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"لم يتم حذف الرسالة: {ex.Message}");
            }
            
            await PaymentRecordsResponse.DisplayPaymentRecords(bot, callbackChatId, requestedPage, subscribers, apiBaseUrl);
            return;
        }

        //Handle Extra Packages Purchase History in message Butons
        else if (callbackQuery.Data!.StartsWith("PackagePurchaseHistory_"))
        {
            int requestedPage = int.Parse(callbackQuery.Data.Replace("PackagePurchaseHistory_", ""));
 
            try
            {
                await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"لم يتم حذف الرسالة: {ex.Message}");
            }

           
            await ExtraPackageMenuResponse.DisplayExtraPackagePurchaseHistory(bot, callbackChatId, requestedPage, subscribers, apiBaseUrl);
            return;
        }
        else if (callbackQuery.Data == "ignore")
        {
            await bot.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
            return;
        }
    }

    // Handling messages
    if (update.Message is not { } message || message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    Console.WriteLine($"Received from {chatId}: {messageText}");

    // To send /start aitomaticaly if the user send anything before sending /start
    if (!userStates.ContainsKey(chatId))
    {        
        await bot.SendTextMessageAsync(
            chatId,
            "أهلاً وسهلاً في بوت مزود إلكم (غير حقيقي) ✨💙 \n نرجو من حضرتك إدخال الرقم الأرضي مع نداء المحافظة",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken
        );

        userStates[chatId] = "awaiting_phone";
        return;
    }

    if (messageText == "/start" || messageText == "تسجيل الدخول")
    {
        userStates.Remove(chatId);
        phoneNumbers.Remove(chatId);
        nationalIds.Remove(chatId);
        tempNewPasswords.Remove(chatId);
        subscribers.Remove(chatId);
 
        userStates[chatId] = "awaiting_phone";
        await bot.SendTextMessageAsync(
            chatId,
            "أهلاً وسهلاً في بوت مزود إلكم (غير حقيقي) ✨💙 \n نرجو من حضرتك إدخال الرقم الأرضي مع نداء المحافظة",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken
        );
        return;
    }


    if (userStates.TryGetValue(chatId, out var state))
    {
        switch (state)
        {
            case "awaiting_phone":
                await Login_PhoneNumberCheck(bot, chatId, messageText);
                break;
            case "awaiting_password":
                await Login_PasswordCheck(bot, chatId, messageText);
                break;
            case "awaiting_national_id":
                await ChangePassword_NationalIdCheck(bot, chatId, messageText, cancellationToken);
                break;
            case "awaiting_new_password":
                await ChangePassword_NewPasswordCheck(bot, chatId, messageText);
                break;
            case "awaiting_new_password_confirmation":
                await ChangePassword_ConfirmNewPasswordCheck(bot, chatId, messageText);
                break;
 
            case "main_menu":
                await ProcessMenuSelection(bot, chatId, messageText, cancellationToken);
                break;
        }
    }
}

//To Handle the menus input.
async Task  ProcessMenuSelection(ITelegramBotClient bot, long chatId, string messageText, CancellationToken cancellationToken)

{
    var subscriberData = subscribers[chatId];

    switch (messageText)
    {
        case "عرض معلومات الاشتراك":
            await SubscriptionMenu.ShowSubscriptionInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "عرض المعلومات المالية":
            await FinancialMenu.ShowFinancialInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "عرض معلومات الباقة":
            await PackageMenu.ShowPackageInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "معلومات الاتصال":
            await ContactInfoMenu.ShowContactInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "تسجيل الخروج":
            await LogoutResponse.Logout(bot, chatId, userStates, phoneNumbers, nationalIds, tempNewPasswords, subscribers); ;
            break;
        case "عرض اسم المستخدم وكلمة المرور":
            await bot.SendTextMessageAsync(chatId, $"اسم المستخدم الخاص بك هو: {subscriberData.AdslUsername}\nكلمة سر اشتراكك هي: {subscriberData.AdslPassword}");
            break;
        case "عرض المعلومات العامة":
            await GeneralInfoMenu.ShowGeneralInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "عرض معلومات الإتصال":
            await SubscriberContactResponse.ShowContactInfo(bot, chatId, subscribers);
            break;
        case "رجوع للقائمة الرئيسية":
            await MainMenu.ShowMainMenu(bot, chatId, messageText);
            break;
        case "عرض الرسوم الشهرية":
            await MonthlyPriceResponse.ShowMonthlyPrice(bot, chatId, subscribers, apiBaseUrl);
            break;
        case "عرض حالة التسديد وموعد الفاتورة القادمة":
            InvoiceMenuResponse invoiceMenuResponse = new InvoiceMenuResponse();
            await invoiceMenuResponse.ShowInvoiceStatusAsync(bot, chatId, subscribers, apiBaseUrl);
            break;
        case "عرض سجل الدفعات":
            await PaymentRecordsResponse.DisplayPaymentRecords(botClient, chatId, 1, subscribers, apiBaseUrl);
            break;
        case "التحقق من الباقة":
            await SubscriberPackageBalanceResponse.ShowPackageBalance(bot, chatId, apiBaseUrl, subscriberData.PhoneNumber);
            break;
        case "شراء حجوم إضافية":
            await ExtraPackageMenuResponse.DisplayExtraPackages(bot, chatId, apiBaseUrl, cancellationToken);
            break;
        case "عرض سجل شراء الباقات":
            await ExtraPackageMenuResponse.DisplayExtraPackagePurchaseHistory(bot, chatId, 1, subscribers, apiBaseUrl);

            break;
        case "تغيير كلمة سر واجهة المشتركين":
            await ChangePassword_NewPasswordRequest(bot, chatId, messageText);
            break;
        case "رجوع لقائمة معلومات الاشتراك":
            await SubscriptionMenu.ShowSubscriptionInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "عرض الشريحة الحالية":
            await bot.SendTextMessageAsync(chatId, $"شريحتك الحالية هي: {subscriberData.PackageName}\n حجم الباقة الأساسي: {subscriberData.MonthlyDataVolumeGb} GB");
            break;
        case "عرض الرصيد المالي":
            await SubscriberPackageBalanceResponse.ShowCurrentBalance(bot, chatId, apiBaseUrl, subscriberData.PhoneNumber);
            break;
        case "عرض حالة الاشتراك":
            await bot.SendTextMessageAsync(chatId, $"اشتراك حضرتك : {subscriberData.SubscriptionStatus}");
            break;
        default:
            await bot.SendTextMessageAsync(chatId, "الرجاء اختيار خيار صحيح من القائمة.");
            break;

    }
}

//Log in process
    async Task Login_PhoneNumberCheck(ITelegramBotClient bot, long chatId, string phoneNumber)
{
    string cleanedPhone = phoneNumber.Trim();

    (bool isValid, string errorMessage, cleanedPhone) = PhoneValidationService.ValidatePhonePrefix(cleanedPhone);

    if (!isValid)
    {
        await bot.SendTextMessageAsync(chatId, errorMessage);
        return;
    }

    var subscriberData = await SubscriberDataService.GetSubscriberData(apiBaseUrl, cleanedPhone);
    if (subscriberData != null)
    {
        phoneNumbers[chatId] = cleanedPhone;
        userStates[chatId] = "awaiting_password";
        subscribers[chatId] = subscriberData;

        await bot.SendTextMessageAsync(chatId, "نرجو من حضرتك إدخال كلمة المرور الخاصة بحسابك في واجهة المشتركين 🙏🏻.",
            replyMarkup: new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("نسيت كلمة المرور؟ 🔒") } })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            });
    }
    else
    {
        await bot.SendTextMessageAsync(chatId, "الرقم خاطئ, حاول مجدداً! 😥.");
    }
}

async Task Login_PasswordCheck(ITelegramBotClient bot, long chatId, string messageText)
{
    if (messageText == "نسيت كلمة المرور؟ 🔒")
    {
        userStates[chatId] = "awaiting_national_id";

        var replyKeyboard = new ReplyKeyboardMarkup(
    new[] { new[] { new KeyboardButton("إلغاء ❌") } }
)
        { ResizeKeyboard = true, OneTimeKeyboard = true };


        await bot.SendTextMessageAsync(
            chatId,
            "الرجاء إدخال الرقم الوطني للتحقق من هويتك 👤.",
            replyMarkup: replyKeyboard
        );

        return;
    }

    var subscriberData = subscribers[chatId];
    string enteredHash = PasswordHashingService.HashPassword(messageText.Trim(), subscriberData.PhoneNumber);

    if (enteredHash == subscriberData.CustomerInterfacePasswordHash)
    {
        await bot.SendTextMessageAsync(chatId, $"أهلاً {subscriberData.FullName} 🌟", replyMarkup: new ReplyKeyboardRemove());

        userStates[chatId] = "main_menu";

        await  MainMenu.ShowMainMenu(bot, chatId, messageText);         
    }
    else
    {
        await bot.SendTextMessageAsync(chatId, "كلمة المرور خاطئة، حاول مجدداً! 😥.");
    }
}


// Change Password Process

async Task ChangePassword_NewPasswordRequest(ITelegramBotClient bot, long chatId, string messageText)
{

    var sub = subscribers[chatId];
    userStates[chatId] = "awaiting_new_password";

    var replyKeyboard = new ReplyKeyboardMarkup(
        new[] { new[] { new KeyboardButton("إلغاء ❌") } }
        )
    { ResizeKeyboard = true, OneTimeKeyboard = true };

    await bot.SendTextMessageAsync(chatId,"أدخل كلمة مرور جديدة (8 محارف على الأقل، تحتوي على رقم وحرف إنكليزي):",
     replyMarkup: replyKeyboard // 🔥 Fixed misplaced comma
 );


}

async Task ChangePassword_NationalIdCheck(ITelegramBotClient bot, long chatId, string messageText, CancellationToken cancellationToken)
{
    var sub = subscribers[chatId];
    if (messageText.Trim() == sub.NationalId)
    {
        userStates[chatId] = "awaiting_new_password";
        await bot.SendTextMessageAsync(chatId, "أدخل كلمة مرور جديدة (8 محارف على الأقل، تحتوي على رقم وحرف إنكليزي):");
        return;
    }
    else if (messageText.Trim() == "إلغاء ❌")
    {
        // If subscriber not found, ask for phone number and area code
        await bot.SendTextMessageAsync(
            chatId,
            "أهلاً وسهلاً في بوت مزود إلكم (غير حقيقي) ✨💙 \n نرجو من حضرتك إدخال الرقم الأرضي مع نداء المحافظة",
            replyMarkup: new ReplyKeyboardRemove()
        );

        userStates[chatId] = "awaiting_phone";
    }
 
    else
    {
        await bot.SendTextMessageAsync(chatId, "الرقم الوطني غير صحيح 😕، حاول مجدداً.");
    }
}


async Task ChangePassword_NewPasswordCheck(ITelegramBotClient bot, long chatId, string messageText)
{
    string newPassword = messageText.Trim();
 
    if (newPassword == "إلغاء ❌")
    {
  
        await bot.SendTextMessageAsync(chatId, "تم إلغاء عملية تغيير كلمة المرور");

        userStates[chatId] = "main_menu";
        await MainMenu.ShowMainMenu(bot, chatId, messageText);
        return;

    }

    if (!IsValidPasswordService.IsValidPassword(newPassword))
    {
        await bot.SendTextMessageAsync(chatId, "كلمة المرور يجب أن تكون 8 محارف على الأقل، وتحتوي على رقم وحرف إنكليزي فقط.");
    
    }

    tempNewPasswords[chatId] = newPassword;
    userStates[chatId] = "awaiting_new_password_confirmation";
    await bot.SendTextMessageAsync(chatId, "أعد إدخال كلمة المرور الجديدة للتأكيد:");
}

async Task ChangePassword_ConfirmNewPasswordCheck(ITelegramBotClient bot, long chatId, string messageText)
{
    if (tempNewPasswords.TryGetValue(chatId, out var firstEntry))
    {
        if (firstEntry == messageText.Trim())
        {
            var sub = subscribers[chatId];

            string apiUrl = $"{apiBaseUrl}/api/Subscribers/update-password/{sub.SubscriberId}";
            var newPassword = messageText.Trim();

            using var http = new HttpClient();
            var json = JsonSerializer.Serialize(newPassword);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await http.PutAsync(apiUrl, content);

            if (result.IsSuccessStatusCode)
            {
                await bot.SendTextMessageAsync(chatId, "✅ تم تحديث كلمة السر بنجاح.",
                    replyMarkup: new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton("تسجيل الدخول") } })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    });
            }
            else
            {
                var reason = await result.Content.ReadAsStringAsync();
                await bot.SendTextMessageAsync(chatId, $"فشل التحديث: {reason}",
                    replyMarkup: new ReplyKeyboardRemove());
            }

            userStates.Remove(chatId);
            phoneNumbers.Remove(chatId);
            nationalIds.Remove(chatId);
            tempNewPasswords.Remove(chatId);
            subscribers.Remove(chatId);
        }
        else
        {
            await bot.SendTextMessageAsync(chatId, "الكلمتان غير متطابقتين 😶، أعد المحاولة:");
        }
    }
}
 

async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine($"An error occurred: {exception.Message}");
}
 













