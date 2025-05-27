using EventBus;
using Microsoft.AspNetCore.SignalR;
using Quartz;
using Services.MessagesService.Entities;
using Services.MessagesService.EventBus.Events;
using Services.MessagesService.Services;
using Services.MessagesService.ServiceWrappers.TopicsService.HttpClient;
using Services.MessagesService.SignalR.Clients;
using Services.MessagesService.SignalR.Hubs;

namespace Services.MessagesService.BackgroundJobs;

public class NewMessageGenerationBackgroundJob(IHubContext<MessagesHub, IMessagesClient> messagesHub, IEventBus eventBus, TopicsServiceHttpClient topicsServiceHttpClient, IMessageService messageService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (context.MergedJobDataMap.TryGetString("TopicId", out var topicId) && !string.IsNullOrEmpty(topicId))
        {
            var topic = await topicsServiceHttpClient.GetTopicById(topicId);
            var lastMessage = (await messageService.GetMessagesByTopicId(topicId)).LastOrDefault(m => m.Creator == MessageCreators.User);
            var lastLlmMessage = (await messageService.GetMessagesByTopicId(topicId)).LastOrDefault(m => m.Creator == MessageCreators.Llm);
            var newMessageEvent = new GenerateTextIntegrationEvent
            {
                LastMessageText = lastMessage is null ? string.Empty : lastMessage.Text,
                Description = topic.Description,
                AdditionalTopicDescription = topic.AdditionalDescription,
                TopicId = topicId,
                PreviousLlmMessage = lastLlmMessage?.Text
            };
            await messagesHub.Clients.Group(topicId).DisableInput();
            await eventBus.Publish(newMessageEvent);
        };
        
    }
}