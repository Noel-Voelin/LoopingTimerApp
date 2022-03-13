using LoopingTimer.Handlers;
using LoopingTimer.Utility;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace LoopingTimer.Models;

/// <summary>
/// Class <c>Timer</c> Model class that represents the state of a single timer and it's underlying scheduler. Also acts as a controller for the quartz scheduler.
/// </summary>
public class Timer
{
	private int intervalInMinutes { get; set; }
	public string id { get; set; }
	
	private IJobDetail? jobDetail;
	private IScheduler? scheduler;
	public readonly MessageDetails MessageDetails;
	private string notificationMessageText;
	private const string PN_MESSAGE_TEXT = "message";
	private const string PN_CHATID= "chatId";
	private const string PN_ID = "id";
	private const int DEFAULT_INTERVAL = 120;


	public Timer(MessageDetails messageDetails)
	{
		LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
		this.MessageDetails = messageDetails;
		id = messageDetails.GetHashCode().ToString();
	}

	/// <summary>
	/// Method <c>SetIntervalFromReply</c> Allows to change the interval of an existing timer.
	/// </summary>
	public void SetIntervalFromReply(string interval)
	{
		intervalInMinutes = int.TryParse(interval, out int configuredInterval)
			? configuredInterval
			: DEFAULT_INTERVAL;
	}
	
	/// <summary>
	/// Method <c>SetSetMessageFromReply</c> Allows to set the reminder message for existing timers
	/// </summary>
	public void SetMessageFromReply(string replyMessage)
	{
		notificationMessageText = replyMessage;
	}

	public async Task start()
	{
		await ScheduleNewTimer(MessageDetails);
	}
	public async void kill()
	{
		if (scheduler == null)
		{
			return;
		}
		
		await scheduler.Shutdown();
	}
	
	private async Task ScheduleNewTimer(MessageDetails messageDetails)
	{
		StdSchedulerFactory factory = new StdSchedulerFactory();
		Task<IScheduler> schedulerTask = factory.GetScheduler();
		
		ITrigger trigger = buildTrigger(messageDetails);
		
		this.jobDetail = BuildJob(messageDetails);
		this.scheduler = await schedulerTask;

		await scheduler.Start();
		if (Environment.GetEnvironmentVariable("ENV") == "Development")
		{
			// Clear all existing jobs and triggers to maintain a clean test scenario
			await scheduler.Clear();
		}

		if (jobDetail != null)
		{
			await scheduler.ScheduleJob(jobDetail, trigger);
		}
	}
	
	private IJobDetail BuildJob(MessageDetails messageDetails)
	{
		IJobDetail job = JobBuilder.Create<NotificationHandler>()
			.WithIdentity(messageDetails.messageId, messageDetails.chatId.ToString())
			.WithDescription(messageDetails.messageText)
			.Build();
		
		job.JobDataMap.Put(PN_CHATID, messageDetails.chatId);
		job.JobDataMap.Put(PN_MESSAGE_TEXT, notificationMessageText);
		job.JobDataMap.Put(PN_ID, id);

		return job;
	}
	
	private ITrigger buildTrigger(MessageDetails messageDetails)
	{
		ITrigger trigger = TriggerBuilder.Create()
			.WithIdentity(messageDetails.messageId, messageDetails.chatId.ToString())
			.StartAt(DateBuilder.FutureDate(intervalInMinutes, IntervalUnit.Minute)).WithSimpleSchedule(schedule => schedule.WithIntervalInMinutes(intervalInMinutes))
			.Build();

		return trigger;
	}
}