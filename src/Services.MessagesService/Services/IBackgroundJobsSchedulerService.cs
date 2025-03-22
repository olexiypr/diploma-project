namespace Services.MessagesService.Services;

public interface IBackgroundJobsSchedulerService
{
    Task ScheduleNewMessageGenerationBackgroundJob(string topicId);
}