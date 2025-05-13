using EventBus;
using Microsoft.AspNetCore.SignalR;
using Services.MessagesService.EventBus.Events;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;
using Services.MessagesService.Services;
using Services.MessagesService.SignalR.Clients;
using Services.MessagesService.SignalR.Hubs;

namespace Services.MessagesService.EventBus.EventHandlers;

public class TextGenerationCompletedIntegrationEventHandler(IHubContext<MessagesHub, IMessagesClient> messagesHub, IMessageService messageService) : IIntegrationEventHandler<TextGenerationCompletedIntegrationEvent>
{
    public async Task Handle(TextGenerationCompletedIntegrationEvent integrationEvent)
    {
        var createdMessage = await messageService.CreateLlmMessage(integrationEvent.TopicId, new CreateMessageRequestModel
        {
            Text = integrationEvent.Text,
        });
        await messagesHub.Clients.All.ReceiveMessage(createdMessage);
    }
}