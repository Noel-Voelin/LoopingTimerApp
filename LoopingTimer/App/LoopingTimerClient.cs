using LoopingTimer.Handlers;
using LoopingTimer.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Timer = LoopingTimer.Models.Timer;

namespace LoopingTimer.App
{
    /// <summary>
    /// Class <c>LoopingTimerClient</c> telegram client wrapper for custom message handling
    /// </summary>
    public class LoopingTimerClient
    {
	    public static event Action<MessageDetails>? SetNotificationMessageEvent;
	    public static event Action<MessageDetails>? SetIntervalEvent;
	    public static event Func<MessageDetails, Timer>? NewTimerEvent;
	    public static event Action<string>? StopTimerEvent;
	    public static event Action<string>? AcknowledgeTimerEvent;

	    private const string ACTION_NAME = "/newtimer";
        
        /// <summary>
        /// Method <c>LoopingTimerClient</c> Constructor that takes a Telegram bot client containing a valid bot token
        /// </summary>
        public LoopingTimerClient(TelegramBotClient botClient)
        {
	        TimerHandler _ = new(botClient);
        }

        /// <summary>
        /// Method <c>HandleUpdateAsync</c> Handles all valid messages and invokes given events
        /// </summary>
        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
	        CancellationToken cancellationToken)
        {

	        switch (update.Type)
	        {
		        case UpdateType.Message:
			        if (update.Message == null)
			        {
				        return Task.CompletedTask;
			        }
			        HandleMessageReceived(update.Message);
			        break;
		        case UpdateType.CallbackQuery:
			        if (update.CallbackQuery == null)
			        {
				        return Task.CompletedTask;
			        }
			        OnCallbackQueryReceived(update.CallbackQuery);
			        break;
	        }
	        return Task.CompletedTask;
        }

        private void OnCallbackQueryReceived(CallbackQuery? callback)
        {
	        if (callback?.Data == null)
	        {
		        return;
	        }
	        
	        var timerId = callback.Data.Split('-').Last();
	        var action = callback.Data.Split('-').First();
	        
	        switch(action)
	        {
		        case "Stop":
			        StopTimerEvent?.Invoke(timerId);
			        break;
		        case "Acknowledged":
			        AcknowledgeTimerEvent?.Invoke(timerId);
			        break;
	        }
        }

        private void HandleMessageReceived(Message updateMessage)
        {
	        MessageDetails messageDetails = new(updateMessage);
	        if (messageDetails.messageText is ACTION_NAME)
	        {
		        NewTimerEvent?.Invoke(messageDetails);
	        } else if (messageDetails.replyText is TimerHandler.SPECIFY_INTERVAL_TEXT)
	        {
		        SetIntervalEvent?.Invoke(messageDetails);
	        } else if (messageDetails.replyText.Split('-').Last() is TimerHandler.SET_MESSAGE_TEXT)
	        {
		        SetNotificationMessageEvent?.Invoke(messageDetails);
	        }
	        
        }
    }
}