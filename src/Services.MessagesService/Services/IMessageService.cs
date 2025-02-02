using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;

namespace Services.MessagesService.Services;

public interface IMessageService
{
    public Task<bool> Create(int topicId, int userId, CreateMessageRequestModel requestModel);
    public Task<MessageResponseModel> GetById(int topicId, string id);
    public Task<IEnumerable<MessageResponseModel>> GetMessagesByTopicId(int topicId);
}