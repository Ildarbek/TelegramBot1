using Newtonsoft.Json;
using Quiz;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class Program
{
    public static int TestNumber = 0;
    //public static List<string> strings = new() {"about", "afraid", "after" };
    public static Dictionary<string, Dictionary<int, List<string>>> myDict = new Dictionary<string, Dictionary<int, List<string>>>(); 
                  //{"about", new Dictionary<int, List<string>>{{1, new List<string>{"кричать", "бояться"}},
                  //{"about", new Dictionary<int, List<string>>{{2, new List<string>{"кричать", "бояться"}} };
    public static Dictionary<int, List<string>> fileList = new Dictionary<int, List<string>>();
    public static Dictionary<string, KeyValuePair<int, string[]>> QADict = new Dictionary<string, KeyValuePair<int, string[]>>();




    public static long chatId = 0;
    private static async Task Main(string[] args)
    {
        var ft = new QAGenerator();
        //Console.WriteLine("directory " + Directory.GetCurrentDirectory());
        QADict = ft.Generate();
        await Console.Out.WriteLineAsync(   $"returned dict cnt {QADict.Count}");
        var botClient = new TelegramBotClient("6390809244:AAEIdXZO3S3zDeDtiQp3_5tMYG_4VCwR7vw");

        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();

        //fileList.Add(1, new List<string> { "о", "кольцо" });
        //var ed = new Dictionary<int, List<string>> { { 2, new List<string> { "кричать", "бояться" } } };

        myDict.Add("about", new Dictionary<int, List<string>> { { 0, new List<string> { "о", "кольцо" } } });
        myDict.Add("afraid", new Dictionary<int, List<string>> { { 1, new List<string> { "кричать", "бояться" } } });

        //fileList.Add(2, new List<string> { "кричать", "бояться" });
        //fileList.Add(1, new List<string> { "vimal", "vilma" });

        //myDict.Add("about", fileList);
        //myDict.Add("afraid", fileList);


        Console.WriteLine($"Start listening for @{me.Username}");
        Console.WriteLine($"me is {JsonConvert.SerializeObject(me)}");
        Console.ReadLine();

        cts.Cancel();

        //update handler 
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync($"Update type {update.Type.ToString()}");
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(botClient, update, cancellationToken),
                UpdateType.CallbackQuery => HandleCallBackQueryAsync(botClient, update, cancellationToken),
                UpdateType.Poll => HandleMessageAsync(botClient, update, cancellationToken),
                UpdateType.PollAnswer => HandleMessageAsync(botClient, update, cancellationToken),
                _ => HandleMessageAsync(botClient, update, cancellationToken)
            };

            try
            {
                await handler;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"exception: {ex.Message}");
            }
        }

        // error handling
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }

    //CallBackQuery
    private static async Task HandleCallBackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync("CallBackQuery");
        await Console.Out.WriteLineAsync($"update.CallbackQuery is {JsonConvert.SerializeObject(update.CallbackQuery)}");
        var callBack = update.CallbackQuery;

        var tests = TestRepository.GetTests();
        Console.WriteLine(TestNumber);
        var test = tests[TestNumber - 1];
        var nextTest = tests[TestNumber];


        await CheckAnswerAsync(test, botClient, callBack, cancellationToken);
        TestNumber++;
        await SendNextQuestion(nextTest, botClient, update, cancellationToken);
    }

    private static async Task SendNextQuestion(Test nextTest, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine("SendNextQuestion");

        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            //row
            new []
            {
                InlineKeyboardButton.WithCallbackData(
                    text: $"{nextTest.A}",
                    callbackData: "A"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(
                    text: $"{nextTest.B}",
                    callbackData: "B"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(
                    text: $"{nextTest.C}",
                    callbackData: "C"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(
                    text: $"{nextTest.D}",
                    callbackData: "D"),
            }
        });



        await Console.Out.WriteLineAsync($"update.Message is {JsonConvert.SerializeObject(update.Message)}");

        if (update.Message is null)
        {
            await botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.From.Id,
                text: $"{nextTest.Question}",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
                );
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: $"{nextTest.Question}",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
                );
        }
    }

    private static async Task CheckAnswerAsync(Test test, ITelegramBotClient botClient, CallbackQuery? callBack, CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync("CheckAnswerAsync invoked");
        await Console.Out.WriteLineAsync($"callBack.Data is {callBack.Data}");

        if (callBack.Data == test.CorrectAnswer)
        {
            await botClient.SendTextMessageAsync(
                chatId: callBack.From.Id,
                text: $"Correct",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: callBack.From.Id,
                text: $"Incorrect",
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
    }

    private static async Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        string? messageText = "";
        await Console.Out.WriteLineAsync("HandleMessageAsync invoked");
        await Console.Out.WriteLineAsync($"update.Type is {update.Type}");

        //if (update.Message is not { } message)
        //    return;
        //if (message.Text is not { } messageText)
        //    return;

        //Console.WriteLine($"Received a '{messageText}' message in chat {update.Message.Chat.Id}.");
        //Console.WriteLine($"myDict[0] is {messageText}");
        //if (update?.Message is not null)
        //{ messageText = update.Message.Text; }

        //var df = myDict.Take(TestNumber);
        await Console.Out.WriteLineAsync($"QADict cnt is {QADict.Count}");

        await Console.Out.WriteLineAsync($"QADict.ElementAt(TestNumber).Key is {QADict.ElementAt(TestNumber).Key}");


        if (update.Type == UpdateType.Message)
        {
            chatId = update.Message.Chat.Id;
            await botClient.SendPollAsync(
                    chatId: chatId,
                    question: QADict.ElementAt(TestNumber).Key,
                    type: PollType.Quiz,
                    correctOptionId: QADict.ElementAt(TestNumber).Value.Key,
                    options: QADict.ElementAt(TestNumber).Value.Value,
                    cancellationToken: cancellationToken);
            TestNumber++;

            //myDict.Take(TestNumber).Last().Key,
            //myDict.Take(TestNumber).Last().Value.Keys.First(),
            //myDict.Take(TestNumber).Last().Value.Values.First()

            //await botClient.SendStickerAsync(
            //     chatId: update.Message.Chat.Id,
            //      sticker: InputFile.FromString("CAACAgIAAxkBAAOEZR-9wzhQFf_87F9ascUIiB_7oWEAAhUAA8A2TxPNVqY7YZ5k5zAE"),
            //       cancellationToken: cancellationToken);


            //        await botClient.SendPollAsync(
            //chatId: update.Message.Chat.Id,
            //question: "Did you ever hear the tragedy of Darth Plagueis The Wise?",
            //type: PollType.Quiz,
            //correctOptionId: 0,

            //options: new[]
            //{
            //    "Yes for the hundredth time!",
            //    "No, who`s that?",
            //    "ddf fvxdfv?"
            //},
            //cancellationToken: cancellationToken);


            //await botClient.SendTextMessageAsync(
            //    chatId:update.Message.Chat.Id,
            //    text:$"Welcome <b> {update.Message.From.FirstName} </b>",
            //    parseMode: ParseMode.Html,
            //    cancellationToken:cancellationToken);
        }
        if (update.Type == UpdateType.Poll)
        {
            await botClient.SendPollAsync(
                    chatId: chatId,
                    question: QADict.ElementAt(TestNumber).Key,
                    type: PollType.Quiz,
                    correctOptionId: QADict.ElementAt(TestNumber).Value.Key,
                    options: QADict.ElementAt(TestNumber).Value.Value,
                    cancellationToken: cancellationToken);
            TestNumber++;
        }

        //TestNumber = 1;
        var tests = TestRepository.GetTests();
        //await SendNextQuestion(tests[0],botClient,update,cancellationToken);
    }
}