using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;

namespace Services.MessagesService.Services;

public interface IMessageService
{
    public Task<MessageResponseModel> Create(string topicId, string cognitoUserId,
        CreateMessageRequestModel requestModel);
    public Task<MessageResponseModel> GetById(string topicId, string id);
    public Task<IEnumerable<MessageResponseModel>> GetMessagesByTopicId(string topicId);
}