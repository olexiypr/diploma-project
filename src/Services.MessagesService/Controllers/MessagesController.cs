using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Services.MessagesService.Extensions;
using Services.MessagesService.HttpClients;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;
using Services.MessagesService.Services;

namespace Services.MessagesService.Controllers;

[ApiController]
[Route("topics/{topicId:int}/[controller]")]
public class MessagesController(IMessageService messageService, IdentityServiceHttpClient identityServiceHttpClient) : ControllerBase
{
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<MessageResponseModel> GetMessageById(int topicId, Guid id)
    {
        return await messageService.GetById(topicId, id.ToString());
    }
    
    [Authorize]
    [HttpPost]
    public async Task<bool> CreateMessageForTopic(int topicId, [FromBody] CreateMessageRequestModel requestModel)
    {
        var usedId = HttpContext.GetUserId();
        var user = await identityServiceHttpClient.GetUserByCognitoId(usedId);
        return await messageService.Create(topicId, user.Id, requestModel);
    }

    [Authorize]
    [HttpGet]
    public async Task<IEnumerable<MessageResponseModel>> GetMessagesByTopicId(int topicId)
    {
        return await messageService.GetMessagesByTopicId(topicId);
    }
}