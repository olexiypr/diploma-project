using Services.MessagesService.Entities;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;

namespace Services.MessagesService.Repositories;

public interface IMessagesRepository
{
    public Task<bool> Create(MessageEntity messageEntity);
    public Task<MessageEntity> GetById(string topicId, string id);
    public Task<IEnumerable<MessageEntity>> GetMessagesByTopicId(string topicId);
}