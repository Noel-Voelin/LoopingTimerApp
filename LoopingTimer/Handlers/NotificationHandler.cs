using LoopingTimer.Models;
using Quartz;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LoopingTimer.Handlers
{
	/// <summary>
	/// Class <c>NotificationHandler</c> Job that is executed by the scheduler to trigger the timer message.
	/// </summary>
	public class NotificationHandler : IJob
	{
		public static event Action<ChatId, IReplyMarkup, string>? SendNotification;

		public Task Execute(IJobExecutionContext context)
		{
			var dataMap = context.MergedJobDataMap;
			var messageDetails = (MessageDetails) dataMap["message"];
			var id = (string)dataMap["id"];


			InlineKeyboardMarkup inlineKeyboard = new(new []
			{
				new []
				{
					InlineKeyboardButton.WithCallbackData(text: "Acknowledged", callbackData: "Acknowledged-"+id),
					InlineKeyboardButton.WithCallbackData(text: "Stop", callbackData: "Stop-"+id)
				},
			});

			SendNotification?.Invoke(messageDetails.chatId, inlineKeyboard, messageDetails.messageText);
			return Task.CompletedTask;
		}
	}
}