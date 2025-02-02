using Services.MessagesService.Mappers;
using Services.MessagesService.Repositories;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;

namespace Services.MessagesService.Services;

public class MessageService(IMessagesRepository messagesRepository, IMessageMapper messageMapper) : IMessageService
{
    public async Task<bool> Create(int topicId, int userId, CreateMessageRequestModel requestModel)
    {
        var messageEntity = messageMapper.Map(userId, topicId, requestModel);
        return await messagesRepository.Create(topicId, userId, messageEntity);
    }

    public async Task<MessageResponseModel> GetById(int topicId, string id)
    {
        var message = await messagesRepository.GetById(topicId, id);
        return messageMapper.Map(message);
    }

    public async Task<IEnumerable<MessageResponseModel>> GetMessagesByTopicId(int topicId)
    {
        var messages = await messagesRepository.GetMessagesByTopicId(topicId);
        return messages.Select(messageMapper.Map);
    }
}