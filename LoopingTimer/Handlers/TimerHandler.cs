using LoopingTimer.Models;
using LoopingTimer.Utility;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Timer = LoopingTimer.Models.Timer;
namespace LoopingTimer.Handlers;

/// <summary>
/// Class <c>TimerHandler</c> creates new timers and handles all modification to existing ones. Also acts as a state-machine. All timers are stored in memory until they are stopped.
/// </summary>
public class TimerHandler
{
	public const string SPECIFY_INTERVAL_TEXT = "Specify the interval in minutes";
	public const string SET_MESSAGE_TEXT = "Set message text";
	
	public static event Action<ChatId, IReplyMarkup?, string>? SendMessageEvent;

	private static readonly Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

	public TimerHandler(TelegramBotClient botClient)
	{
		MessageHandler _ = new(botClient);
		App.LoopingTimerClient.SetNotificationMessageEvent += onSetNotificationMessage;		
		App.LoopingTimerClient.NewTimerEvent += onNewTimer;
		App.LoopingTimerClient.SetIntervalEvent += onSetInterval;
		App.LoopingTimerClient.StopTimerEvent += onStopTimer;
		App.LoopingTimerClient.AcknowledgeTimerEvent += onAcknowledgement;
	}

	private async void onSetNotificationMessage(MessageDetails messageDetail)
	{
		if (!timers.TryGetValue(messageDetail.messageId, out var timer)) {
			return;
		}
		timer.SetMessageFromReply(messageDetail.messageText);
		await timer.start();

		InlineKeyboardMarkup inlineKeyboard = new(new []
		{
			new []
			{
				InlineKeyboardButton.WithCallbackData(text: "Stop", callbackData: "Stop-"+timer.id),
			},
		});
		SendMessageEvent?.Invoke(messageDetail.chatId, inlineKeyboard, "Timer has been scheduled");
	}


	private async void onAcknowledgement(string timerId)
	{			       
		if (!timers.TryGetValue(timerId, out var timer)) {
			return;
		}
		
		await timer.start();
		SendMessageEvent?.Invoke(timer.MessageDetails.chatId, null, "Timer has been scheduled again");
	}

	private void onStopTimer(string timerId)
	{
		
		if (!timers.TryGetValue(timerId, out var timer)) {
			return;
		}
		timer.kill();
		timers.Remove(timerId);
		
		SendMessageEvent?.Invoke(timer.MessageDetails.chatId, null, "Timer has been stopped");
	}

	private static void onSetInterval(MessageDetails messageDetails)
	{
		if (!timers.TryGetValue(messageDetails.messageId, out var timer)) {
			return;
		}
		timer.SetIntervalFromReply(messageDetails.messageText);
		
		InlineKeyboardMarkup inlineKeyboard = new(new []
		{
			new []
			{
				InlineKeyboardButton.WithCallbackData(text: "Stop", callbackData: "Stop-"+timer.id),
			},
		});
		SendMessageEvent?.Invoke(messageDetails.chatId, new ForceReplyMarkup(), $"[{timer.id}]-{SET_MESSAGE_TEXT}");
	}

	private static Timer onNewTimer(MessageDetails messageDetails)
	{
		Timer timer = new Timer(messageDetails);
		if(!timers.ContainsKey(timer.id)){
			timers[timer.id] = timer;
		}
		SendMessageEvent?.Invoke(messageDetails.chatId, new ForceReplyMarkup(), $"[{timer.id}]-{SPECIFY_INTERVAL_TEXT}");
		return timer;
	}
}