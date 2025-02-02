using Services.MessagesService.Entities;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;

namespace Services.MessagesService.Mappers;

public class MessageMapper : IMessageMapper
{
    public MessageEntity Map(int createdBy, int topicId, CreateMessageRequestModel model)
    {
        return new MessageEntity
        {
            Id = Guid.NewGuid().ToString(),
            CreatedBy = createdBy,
            TopicId = topicId,
            Text = model.Text,
            MessageCreator = MessageCreators.User
        };
    }

    public MessageResponseModel Map(MessageEntity entity)
    {
        return new MessageResponseModel
        {
            Id = entity.Id,
            TopicId = entity.TopicId,
            CreatedBy = entity.CreatedBy,
            DateCreated = entity.DateCreated,
            Text = entity.Text
        };
    }
}