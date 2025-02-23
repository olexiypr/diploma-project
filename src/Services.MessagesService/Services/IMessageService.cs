using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;

namespace Services.MessagesService.Services;

public interface IMessageService
{
    public Task<MessageResponseModel> Create(int topicId, string cognitoUserId, CreateMessageRequestModel requestModel);
    public Task<MessageResponseModel> GetById(int topicId, string id);
    public Task<IEnumerable<MessageResponseModel>> GetMessagesByTopicId(int topicId);
}