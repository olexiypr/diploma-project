using Services.MessagesService.Entities;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;

namespace Services.MessagesService.Mappers;

public interface IMessageMapper
{
    MessageEntity Map(int createdBy, int topicId, CreateMessageRequestModel model);
    MessageResponseModel Map(MessageEntity entity);
}