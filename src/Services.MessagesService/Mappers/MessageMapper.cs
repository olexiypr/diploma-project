using Services.MessagesService.Entities;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;
using Services.MessagesService.ServiceWrappers.IdentityService.Models;

namespace Services.MessagesService.Mappers;

public class MessageMapper : IMessageMapper
{
    public MessageEntity Map(UserModel createdBy, string topicId, CreateMessageRequestModel model)
    {
        return new MessageEntity
        {
            Id = Guid.NewGuid().ToString(),
            CreatedBy = createdBy.Id,
            CreatorName = createdBy.Name,
            TopicId = topicId,
            Text = model.Text,
            MessageCreator = MessageCreators.User
        };
    }

    public MessageEntity MapLlm(string topicId, CreateMessageRequestModel model)
    {
        return new MessageEntity
        {
            Id = Guid.NewGuid().ToString(),
            CreatedBy = 0,
            TopicId = topicId,
            Text = model.Text,
            MessageCreator = MessageCreators.Llm
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
            CreatorName = entity.CreatorName,
            Text = entity.Text,
            Creator = entity.MessageCreator
        };
    }
}