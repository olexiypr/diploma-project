using Quartz;

namespace Services.MessagesService.BackgroundJobs;

public class NewMessageGenerationBackgroundJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        if (context.MergedJobDataMap.TryGetString("TopicId", out var topicId))
        {
            
        }
        return Task.CompletedTask;
    }
}