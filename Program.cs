using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups; 
using System.Text;
using System.Text.Json; 
using ElcomCustomerTelegramBot.Services;
using ElcomCustomerTelegramBot.Models;
using ElcomCustomerTelegramBot.MenuServices;
using ElcomCustomerTelegramBot.MenuResponses;


var botClient = new TelegramBotClient("7997457895:AAFRkiMzRoy8KbkGPhp0Xf93294j_r29RnA");
var apiBaseUrl = "http://elcomapi.runasp.net";

var userStates = new Dictionary<long, string>();
var phoneNumbers = new Dictionary<long, string>();
var nationalIds = new Dictionary<long, string>();
var subscribers = new Dictionary<long, Subscriber>();
var tempNewPasswords = new Dictionary<long, string>();

using var cts = new CancellationTokenSource();
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token
);




Console.WriteLine("Bot is running. Press Enter to exit.");
Console.ReadLine();

async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
{
    // التعامل مع ضغط زر InlineKeyboard
    if (update.Type == UpdateType.CallbackQuery)
    {
        var callbackQuery = update.CallbackQuery!;
        var callbackChatId = callbackQuery.Message!.Chat.Id;

        if (callbackQuery.Data!.StartsWith("selectExtra_"))
        {
            int extraPackageId = int.Parse(callbackQuery.Data.Replace("selectExtra_", ""));
            await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken);
            await ExtraPackageMenuResponse.HandleExtraPackageSelectionAsync(bot, callbackChatId, extraPackageId, apiBaseUrl, cancellationToken);
            return;
        }
        else if (callbackQuery.Data!.StartsWith("confirmExtra_"))
        {
            int extraPackageId = int.Parse(callbackQuery.Data.Replace("confirmExtra_", ""));
            await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken);
            await ExtraPackageMenuResponse.ConfirmExtraPackagePurchaseAsync(bot, callbackChatId, extraPackageId, subscribers, apiBaseUrl, cancellationToken);
            return;
        }
        else if (callbackQuery.Data == "cancelExtra")
        {
            await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken);
            await ExtraPackageMenuResponse.CancelExtraPackagePurchaseAsync(bot, callbackChatId, cancellationToken);
            return;
        }

        // التحقق من نوع الصفحة (فواتير أو باقات)
        if (callbackQuery.Data!.StartsWith("paymentPage_"))
        {
            int requestedPage = int.Parse(callbackQuery.Data.Replace("paymentPage_", ""));

            // حذف الرسالة القديمة
            try
            {
                await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"لم يتم حذف الرسالة: {ex.Message}");
            }

            // عرض سجلات الفواتير
            await PaymentRecordsResponse.DisplayPaymentRecords(bot, callbackChatId, requestedPage, subscribers, apiBaseUrl);
            return;
        }
        else if (callbackQuery.Data!.StartsWith("extraPage_"))
        {
            int requestedPage = int.Parse(callbackQuery.Data.Replace("extraPage_", ""));

            // حذف الرسالة القديمة
            try
            {
                await bot.DeleteMessageAsync(callbackChatId, callbackQuery.Message.MessageId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"لم يتم حذف الرسالة: {ex.Message}");
            }

            // عرض سجلات شراء الباقات
            await ExtraPackageMenuResponse.DisplayExtraPackagePurchaseRecords(bot, callbackChatId, requestedPage, subscribers, apiBaseUrl);
            return;
        }
        else if (callbackQuery.Data == "noop")
        {
            await bot.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: cancellationToken);
            return;
        }
    }

    // التعامل مع الرسائل النصية
    if (update.Message is not { } message || message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    Console.WriteLine($"Received from {chatId}: {messageText}");

    // إذا كانت الرسالة هي أول رسالة (لم يتم إرسال /start من قبل)
    if (!userStates.ContainsKey(chatId))
    {
        // إرسال /start تلقائيًا
        await bot.SendTextMessageAsync(
            chatId,
            "أهلاً وسهلاً في بوت مزود إلكم (غير حقيقي) ✨💙 \n نرجو من حضرتك إدخال الرقم الأرضي مع نداء المحافظة",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken
        );

        // تحديد الحالة الأولى للمستخدم (أي لم يرسل /start بعد)
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

        // إرسال رسالة الترحيب مع إزالة لوحة المفاتيح
        userStates[chatId] = "awaiting_phone";
        await bot.SendTextMessageAsync(
            chatId,
            "أهلاً وسهلاً في بوت مزود إلكم (غير حقيقي) ✨💙 \n نرجو من حضرتك إدخال الرقم الأرضي مع نداء المحافظة",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken
        );
        return;
    }

    if (userStates[chatId] == "awaiting_new_password")
    {
        tempNewPasswords.Remove(chatId);

        // إرسال رسالة إلغاء العملية
        await bot.SendTextMessageAsync(chatId, $"تم إلغاء عملية تغيير كلمة المرور");
        userStates[chatId] = "main_menu";
        await SubscriptionMenuService.ShowSubscriptionInfoMenu(bot, chatId, messageText, subscribers);

        return;
    }

    // منطق الحالات بناءً على الحالة الحالية للمستخدم
    if (userStates.TryGetValue(chatId, out var state))
    {
        switch (state)
        {
            case "awaiting_phone":
                await HandlePhoneInputAsync(bot, chatId, messageText);
                break;
            case "awaiting_password":
                await HandlePasswordInputAsync(bot, chatId, messageText);
                break;
            case "awaiting_national_id":
                await HandleNationalIdInputAsync(bot, chatId, messageText);
                break;
            case "awaiting_new_password":
                await HandleNewPasswordInputAsync(bot, chatId, messageText);
                break;
            case "awaiting_new_password_confirmation":
                await HandleNewPasswordConfirmationAsync(bot, chatId, messageText);
                break;
            case "main_menu":
                await HandleMenuInputAsync(bot, chatId, messageText, cancellationToken);
                break;
        }
    }
}



async Task HandlePhoneInputAsync(ITelegramBotClient bot, long chatId, string phoneNumber)
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

async Task HandlePasswordInputAsync(ITelegramBotClient bot, long chatId, string messageText)
{
    if (messageText == "نسيت كلمة المرور؟ 🔒")
    {
        userStates[chatId] = "awaiting_national_id";
        await bot.SendTextMessageAsync(chatId, "الرجاء إدخال الرقم الوطني للتحقق من هويتك 👤.");
        return;
    }

    var subscriberData = subscribers[chatId];
    string enteredHash = PasswordHashingService.HashPassword(messageText.Trim(), subscriberData.PhoneNumber);

    if (enteredHash == subscriberData.CustomerInterfacePasswordHash)
    {
        await bot.SendTextMessageAsync(chatId, $"أهلاً {subscriberData.FullName} 🌟", replyMarkup: new ReplyKeyboardRemove());

        userStates[chatId] = "main_menu";

        await  MainMenuService.ShowMainMenu(bot, chatId, messageText);         
    }
    else
    {
        await bot.SendTextMessageAsync(chatId, "كلمة المرور خاطئة، حاول مجدداً! 😥.");
    }
}

async Task HandleNationalIdInputAsync(ITelegramBotClient bot, long chatId, string messageText)
{
    var sub = subscribers[chatId];
    if (messageText.Trim() == sub.NationalId)
    {
        userStates[chatId] = "awaiting_new_password";
        await bot.SendTextMessageAsync(chatId, "أدخل كلمة مرور جديدة (8 محارف على الأقل، تحتوي على رقم وحرف إنكليزي):");
    }
    else
    {
        await bot.SendTextMessageAsync(chatId, "الرقم الوطني غير صحيح 😕، حاول مجدداً.");
    }
}

async Task HandleNewPasswordInputAfterSigningInAsync(ITelegramBotClient bot, long chatId, string messageText)
{

    var sub = subscribers[chatId];    
    userStates[chatId] = "awaiting_new_password";
    await bot.SendTextMessageAsync(chatId, "أدخل كلمة مرور جديدة (8 محارف على الأقل، تحتوي على رقم وحرف إنكليزي):");
     
}


async Task HandleNewPasswordInputAsync(ITelegramBotClient bot, long chatId, string messageText)
{
    string newPassword = messageText.Trim();


    // تحقق من الرسالة الخاصة بالرجوع
    if (newPassword == "رجوع لقائمة معلومات الاشتراك")
    {
        // إرسال رسالة إلغاء العملية
        await bot.SendTextMessageAsync(chatId, $"تم إلغاء عملية تغيير كلمة المرور");

        // تغيير حالة المستخدم
        userStates[chatId] = "new_password_confirmation_canceled";

      
        return; // إنهاء الدالة هنا وإيقاف باقي التحقق
    }
 


    if (!IsValidPasswordService.IsValidPassword(newPassword))
    {
        await bot.SendTextMessageAsync(chatId, "كلمة المرور يجب أن تكون 8 محارف على الأقل، وتحتوي على رقم وحرف إنكليزي فقط.");
        return;
    }
  
    tempNewPasswords[chatId] = newPassword;
    userStates[chatId] = "awaiting_new_password_confirmation";
    await bot.SendTextMessageAsync(chatId, "أعد إدخال كلمة المرور الجديدة للتأكيد:");
}

async Task HandleNewPasswordConfirmationAsync(ITelegramBotClient bot, long chatId, string messageText)
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

 








async Task HandleMenuInputAsync(ITelegramBotClient bot, long chatId, string messageText, CancellationToken cancellationToken)

{
    var subscriberData = subscribers[chatId];

    switch (messageText)
    {
        case "عرض معلومات الاشتراك":
            await SubscriptionMenuService.ShowSubscriptionInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "عرض المعلومات المالية":
            await FinancialMenuService.ShowFinancialInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "عرض معلومات الباقة":
            await PackageMenuService.ShowPackageInfoMenu(bot, chatId , messageText, subscribers);
            break;
        case "معلومات الاتصال":
            await ContactInfoMenuService.ShowContactInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "تسجيل الخروج":
            await LogoutResponse.Logout(bot, chatId, userStates, phoneNumbers, nationalIds, tempNewPasswords, subscribers); ;
            break;
        case "عرض اسم المستخدم وكلمة المرور":
            await bot.SendTextMessageAsync(chatId, $"اسم المستخدم الخاص بك هو: {subscriberData.AdslUsername}\nكلمة سر اشتراكك هي: {subscriberData.AdslPassword}");
            break;
        case "عرض المعلومات العامة":
            await GeneralInfoMenuService.ShowGeneralInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "عرض معلومات الإتصال":
            await SubscriberContactResponse.ShowContactInfo(bot, chatId, subscribers);
            break;
        case "رجوع للقائمة الرئيسية":
            await MainMenuService.ShowMainMenu(bot, chatId, messageText);
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
            await SubscriberBalancePackageResponse.ShowPackageBalanceAsync(bot, chatId,apiBaseUrl, subscriberData.PhoneNumber);
            break;
        case "شراء حجوم إضافية":
            await ExtraPackageMenuResponse.DisplayExtraPackagesAsync(bot, chatId, apiBaseUrl, cancellationToken);
            break;
        case "عرض سجل شراء الباقات":
            await ExtraPackageMenuResponse.DisplayExtraPackagePurchaseRecords(bot, chatId, 1, subscribers, apiBaseUrl);

            break;
        case "تغيير كلمة سر واجهة المشتركين":
            await HandleNewPasswordInputAfterSigningInAsync(bot, chatId, messageText);
            break;
        case "رجوع لقائمة معلومات الاشتراك":
            await SubscriptionMenuService.ShowSubscriptionInfoMenu(bot, chatId, messageText, subscribers);
            break;
        case "عرض الشريحة الحالية":
            await bot.SendTextMessageAsync(chatId, $"شريحتك الحالية هي: {subscriberData.PackageName}");
            break;
        case "عرض الرصيد المالي":
            await SubscriberBalancePackageResponse.ShowCurrentBalanceAsync(bot, chatId, apiBaseUrl, subscriberData.PhoneNumber);
            break;
        case "عرض حالة الاشتراك":
            await bot.SendTextMessageAsync(chatId, $"اشتراك حضرتك : {subscriberData.SubscriptionStatus}");
            break;
        default:
            await bot.SendTextMessageAsync(chatId, "الرجاء اختيار خيار صحيح من القائمة.");
            break;

    }
}
 
 

 

  
 


async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine($"An error occurred: {exception.Message}");
}













