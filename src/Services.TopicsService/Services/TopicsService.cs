using Services.Topics.Entities;

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

    public async Task<TopicEntity> Create(string title)
    {
        return await mongoDbService.Create(title);
    }
}