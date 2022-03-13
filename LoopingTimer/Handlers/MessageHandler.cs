using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LoopingTimer.Handlers;


public class MessageHandler
{
	private readonly TelegramBotClient _botClient;
	public MessageHandler(TelegramBotClient botClient)
	{
		_botClient = botClient;
		TimerHandler.SendMessageEvent += OnSendMessageEvent;
		NotificationHandler.SendNotification += OnSendMessageEvent;
	}
	
	private void OnSendMessageEvent(ChatId chatId, IReplyMarkup reply, string text)
	{
		_botClient.SendTextMessageAsync(
			chatId: chatId,
			text: text,
			replyMarkup: reply);
	}
}