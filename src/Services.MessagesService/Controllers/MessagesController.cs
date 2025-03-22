using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.MessagesService.ResponseModels;
using Services.MessagesService.Services;

namespace Services.MessagesService.Controllers;

[ApiController]
[Route("topics/{topicId}/[controller]")]
public class MessagesController(IMessageService messageService) : ControllerBase
{
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<MessageResponseModel> GetMessageById(string topicId, Guid id)
    {
        return await messageService.GetById(topicId, id.ToString());
    }

    [HttpGet]
    public async Task<IEnumerable<MessageResponseModel>> GetMessagesByTopicId(string topicId)
    {
        return await messageService.GetMessagesByTopicId(topicId);
    }
}