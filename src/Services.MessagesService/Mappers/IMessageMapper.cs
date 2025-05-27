using Services.MessagesService.Entities;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;
using Services.MessagesService.ServiceWrappers.IdentityService.Models;

namespace Services.MessagesService.Mappers;

public interface IMessageMapper
{
    MessageEntity Map(UserModel createdBy, string topicId, CreateMessageRequestModel model);
    MessageEntity MapLlm(string topicId, CreateMessageRequestModel model);
    MessageResponseModel Map(MessageEntity entity);
}