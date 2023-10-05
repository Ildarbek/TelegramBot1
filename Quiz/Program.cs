using Quiz;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class Program
{
    public static int TestNumber;
    private static async Task Main(string[] args)
    {
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

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        cts.Cancel();

        //update handler 
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(botClient, update, cancellationToken),
                UpdateType.CallbackQuery => HandleCallBackQueryAsync(botClient, update, cancellationToken),
                
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
        var callBack = update.CallbackQuery;

        var tests = TestRepository.GetTests();
        Console.WriteLine(TestNumber);
        var test = tests[TestNumber - 1];
        var nextTest = tests[TestNumber];


        await CheckAnswerAsync(test,botClient, callBack, cancellationToken);
        TestNumber++;
        await SendNextQuestion(nextTest, botClient, update, cancellationToken);
    }

    private static async Task SendNextQuestion(Test nextTest, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

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

        if(update.Message is null )
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
        if(callBack.Data == test.CorrectAnswer)
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
        if (update.Message is not { } message)
            return;
        if (message.Text is not { } messageText)
            return;
        
        Console.WriteLine($"Received a '{messageText}' message in chat {update.Message.Chat.Id}.");

        if(messageText == "/start")
        {
            await botClient.SendTextMessageAsync(
                chatId:update.Message.Chat.Id,
                text:$"Welcome <b> {update.Message.From.FirstName} </b>",
                parseMode: ParseMode.Html,
                cancellationToken:cancellationToken);
        }

        TestNumber = 1;
        var tests = TestRepository.GetTests();
        await SendNextQuestion(tests[0],botClient,update,cancellationToken);
    }
}