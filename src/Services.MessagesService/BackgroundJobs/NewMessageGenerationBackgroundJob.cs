using Quartz;

namespace Services.MessagesService.BackgroundJobs;

public class NewMessageGenerationBackgroundJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        if (context.MergedJobDataMap.TryGetIntValue("TopicId", out var topicId))
        {
            
        }
        return Task.CompletedTask;
    }
}