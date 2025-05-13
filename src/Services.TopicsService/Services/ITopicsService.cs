using Services.Topics.Entities;
using Services.Topics.RequestModels;

namespace Services.Topics.Services;

public interface ITopicsService
{
    Task<List<TopicEntity>> GetAll();
    Task<TopicEntity> GetById(string id);
    Task<TopicEntity> Create(CreateTopicRequestModel createTopicRequestModel);
}