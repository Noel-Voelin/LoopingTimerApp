using Telegram.Bot.Types;

namespace LoopingTimer.Models
{

	/// <summary>
	/// Class <c>MessageDetails</c> Model Class to extract relevant information from Telegram Message
	/// </summary>
	public class MessageDetails
	{
		public string messageId { get;}
		public ChatId chatId { get; }

		public string messageText { get; }
		public string replyText { get; }

		public MessageDetails(Message message)
		{
			this.chatId = message.Chat.Id;
			this.messageId = message.ReplyToMessage?.Text?.Split("-").First().Trim('[',']') ?? message.MessageId.ToString();
			this.messageText = message.Text ?? string.Empty;
			this.replyText = message.ReplyToMessage?.Text?.Split("-").Last() ?? string.Empty;
		}

	}

}
