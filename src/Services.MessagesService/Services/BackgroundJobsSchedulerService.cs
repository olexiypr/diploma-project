using Microsoft.Extensions.Options;
using Quartz;
using Services.MessagesService.BackgroundJobs;
using Services.MessagesService.Settings;

namespace Services.MessagesService.Services;

public class BackgroundJobsSchedulerService(ISchedulerFactory schedulerFactory, IOptions<NewMessageGenerationBackgroundJobSettings> generationJobSettings) : IBackgroundJobsSchedulerService
{
    private const string MessageGenerationBackgroundJobDataMapKey = "TopicId";
    public async Task ScheduleNewMessageGenerationBackgroundJob(string topicId)
    {
        //var nextFireDatetime = DateTime.UtcNow + generationJobSettings.Value.GenerateNewMessageOffset;
        var nextFireDatetime = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        var scheduler = await schedulerFactory.GetScheduler();
        var jobs = await scheduler.GetCurrentlyExecutingJobs();
        var existingJob = jobs.FirstOrDefault(j => j.MergedJobDataMap.ContainsKey(MessageGenerationBackgroundJobDataMapKey) &&
                                                   j.MergedJobDataMap[MessageGenerationBackgroundJobDataMapKey].ToString() == topicId.ToString());
        ITrigger trigger;
        if (existingJob is not null)
        {
            trigger = TriggerBuilder.Create()
                .ForJob(existingJob.JobDetail)
                .StartAt(nextFireDatetime)
                .Build();
            await scheduler.RescheduleJob(existingJob.Trigger.Key, trigger);
            return;
        }
        var jobData = new JobDataMap
        {
            {MessageGenerationBackgroundJobDataMapKey, topicId},
        };
        var jobDetail = JobBuilder.Create<NewMessageGenerationBackgroundJob>()
            .UsingJobData(jobData)
            .Build();
        
        trigger = TriggerBuilder.Create()
            .ForJob(jobDetail)
            .StartAt(nextFireDatetime)
            .Build();
        await scheduler.ScheduleJob(jobDetail, trigger);
    }
}