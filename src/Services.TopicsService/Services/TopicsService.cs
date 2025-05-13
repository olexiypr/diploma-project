using Services.Topics.Entities;
using Services.Topics.RequestModels;

namespace Services.Topics.Services;

public class TopicsService(MongoDbService mongoDbService) : ITopicsService
{
    public async Task<List<TopicEntity>> GetAll()
    {
        return await mongoDbService.GetAll();
    }

    public async Task<TopicEntity> GetById(string id)
    {
        return await mongoDbService.GetById(id);
    }

    public async Task<TopicEntity> Create(CreateTopicRequestModel createTopicRequestModel)
    {
        return await mongoDbService.Create(createTopicRequestModel);
    }
}