using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.MessagesService.ResponseModels;
using Services.MessagesService.Services;

namespace Services.MessagesService.Controllers;

[ApiController]
[Route("topics/{topicId:int}/[controller]")]
public class MessagesController(IMessageService messageService) : ControllerBase
{
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<MessageResponseModel> GetMessageById(int topicId, Guid id)
    {
        return await messageService.GetById(topicId, id.ToString());
    }

    [Authorize]
    [HttpGet]
    public async Task<IEnumerable<MessageResponseModel>> GetMessagesByTopicId(int topicId)
    {
        return await messageService.GetMessagesByTopicId(topicId);
    }
}