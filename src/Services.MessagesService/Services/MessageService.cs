using Services.MessagesService.Mappers;
using Services.MessagesService.Repositories;
using Services.MessagesService.RequestModels;
using Services.MessagesService.ResponseModels;
using Services.MessagesService.ServiceWrappers.IdentityService.HttpClient;

namespace Services.MessagesService.Services;

public class MessageService(IMessagesRepository messagesRepository,
    IMessageMapper messageMapper, IdentityServiceHttpClient identityServiceHttpClient, IBackgroundJobsSchedulerService backgroundJobsSchedulerService) : IMessageService
{
    public async Task<MessageResponseModel> Create(string topicId, string cognitoUserId,
        CreateMessageRequestModel requestModel)
    {
        var user = await identityServiceHttpClient.GetUserByCognitoId(cognitoUserId);
        var messageEntity = messageMapper.Map(user.Id, topicId, requestModel);
        await messagesRepository.Create(messageEntity);
        //await backgroundJobsSchedulerService.ScheduleNewMessageGenerationBackgroundJob(topicId);
        return messageMapper.Map(messageEntity);
    }
    
    public async Task<MessageResponseModel> CreateLlmMessage(string topicId, CreateMessageRequestModel requestModel)
    {
        var messageEntity = messageMapper.MapLlm(topicId, requestModel);
        await messagesRepository.Create(messageEntity);
        await backgroundJobsSchedulerService.ScheduleNewMessageGenerationBackgroundJob(topicId);
        return messageMapper.Map(messageEntity);
    }

    public async Task<MessageResponseModel> GetById(string topicId, string id)
    {
        var message = await messagesRepository.GetById(topicId, id);
        return messageMapper.Map(message);
    }

    public async Task<IEnumerable<MessageResponseModel>> GetMessagesByTopicId(string topicId)
    {
        var messages = await messagesRepository.GetMessagesByTopicId(topicId);
        return messages.Select(messageMapper.Map);
    }
}