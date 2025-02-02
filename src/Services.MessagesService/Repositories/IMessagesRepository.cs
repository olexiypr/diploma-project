using Services.MessagesService.Entities;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;

namespace Services.MessagesService.Repositories;

public interface IMessagesRepository
{
    public Task<bool> Create(int topicId, int usedId, MessageEntity messageEntity);
    public Task<MessageEntity> GetById(int topicId, string id);
    public Task<IEnumerable<MessageEntity>> GetMessagesByTopicId(int topicId);
}