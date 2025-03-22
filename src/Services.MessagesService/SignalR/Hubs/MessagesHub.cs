using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Services.MessagesService.Extensions;
using Services.MessagesService.RequestModels;
using Services.MessagesService.Services;
using Services.MessagesService.SignalR.Clients;

namespace Services.MessagesService.SignalR.Hubs;

[Authorize]
public class MessagesHub(IMessageService messageService) : Hub<IMessagesClient>
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task OpenTopic(int topicId)
    {
        //TODO: Verify that topic exists
        await Groups.AddToGroupAsync(Context.ConnectionId, topicId.ToString());
    }

    public async Task CloseTopic(int topicId)
    {
        //TODO: Verify that topic exists
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, topicId.ToString());
    }

    public async Task CreateMessage(string topicId, CreateMessageRequestModel requestModel)
    {
        //TODO: Verify that topic exists
        var cognitoUserId = Context.GetHttpContext()?.GetUserId();
        if (string.IsNullOrEmpty(cognitoUserId))
        {
            throw new UnauthorizedAccessException();
        }
        var result = await messageService.Create(topicId, cognitoUserId, requestModel);
        await Clients.Group(topicId.ToString()).ReceiveMessageCreated(result);
        await Clients.GroupExcept(topicId.ToString(), []).ReceiveMessageCreated(result);
    }
}