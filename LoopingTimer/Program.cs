using LoopingTimer.App;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;

namespace LoopingTimer;

/// <summary>
/// Class <c>LoopingTimerApplication</c> Console application entry class.
/// </summary>
public static class LoopingTimerApplication
{
    private static readonly TelegramBotClient BotClient = new TelegramBotClient("Get your own token");
    private static readonly LoopingTimerClient LoopingTimer = new LoopingTimerClient(BotClient);

    /// <summary>
    /// Method <c>Main</c> wrapper for MainAsync to allow async entry method (maybe someone knows a better way)
    /// </summary>
    public static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }
    
    private static async Task MainAsync()
    {
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions();

        // Register bot handler methods along with options
        BotClient.StartReceiving(
            LoopingTimer.HandleUpdateAsync,
            HandleAsyncTelegramErrors,
            receiverOptions,
            cancellationToken: cts.Token);
        
        
        var me = await BotClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();
        cts.Cancel();
    }

    
    private static Task HandleAsyncTelegramErrors(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}
