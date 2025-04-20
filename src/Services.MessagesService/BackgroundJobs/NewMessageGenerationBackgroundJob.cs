using EventBus;
using Quartz;
using Services.MessagesService.EventBus.Events;

namespace Services.MessagesService.BackgroundJobs;

public class NewMessageGenerationBackgroundJob(IEventBus eventBus) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (context.MergedJobDataMap.TryGetString("TopicId", out var topicId))
        {
            var createNewTopic = new 
            {
                TopicId = "1",
                TopicName = "Ukrainian Cossacks",
                Description = "This story unfolds in the late 16th to early 18th century, a time when the Ukrainian Cossacks defended their homeland against foreign invaders and internal threats. The setting is the vast steppes of what is now central and eastern Ukraine, with frequent mentions of the Dnipro River, frontier villages, and fortified camps known as \"sich.\" The main idea revolves around a group of Cossacks—free warriors and defenders of the Ukrainian people—who fight to protect their land, freedom, and honor. Their enemies include Tatar raiders, Polish nobles, and later, Ottoman and Muscovite forces, depending on the direction of the narrative. The Cossacks are portrayed as courageous, loyal, and deeply tied to the land and the people they protect.",
                AdditionalTopicDescription = "Players contribute to a continuous story, each message adding new twists, characters, or events while preserving historical mood and logical progression. The tone of the story should feel dramatic, grounded in history, with moments of heroism, sacrifice, and brotherhood. Messages can involve battles, rescues, strategy meetings, travels through the steppes, or even moments of reflection around the campfire. The story should respect the historical and cultural essence of the Cossack spirit while leaving room for creative storytelling."
            };
            var textToTest =
                "The sun dipped below the Dnipro River as Hetman Ostap surveyed the horizon, his saber resting against his hip. Smoke curled in the distance—a sign that Tatar raiders had set another village ablaze. He turned to his brothers-in-arms, their faces hardened by years of war, and raised his fist. \"Tonight, we ride to defend our land, or die trying!";
            var newMessageEvent = new GenerateTextIntegrationEvent
            {
                LastMessageText = textToTest,
                Description = createNewTopic.Description,
                AdditionalTopicDescription = createNewTopic.AdditionalTopicDescription,
            };
            Console.WriteLine($"Job NewMessageGenerationBackgroundJob has been fired at {DateTime.Now}");
            await eventBus.Publish(newMessageEvent);
        }
    }
}