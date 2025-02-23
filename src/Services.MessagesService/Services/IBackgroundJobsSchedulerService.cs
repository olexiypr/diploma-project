namespace Services.MessagesService.Services;

public interface IBackgroundJobsSchedulerService
{
    Task ScheduleNewMessageGenerationBackgroundJob(int topicId);
}