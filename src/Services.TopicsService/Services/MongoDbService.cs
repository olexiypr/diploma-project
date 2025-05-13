using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Services.Topics.Entities;
using Services.Topics.RequestModels;

namespace Services.Topics.Services;

public class MongoDbService
{
    private readonly IMongoCollection<TopicEntity> _topics;

    public MongoDbService(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionUrl);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _topics = database.GetCollection<TopicEntity>(settings.Value.CollectionName);
    }

    public async Task SeedData()
    {
        var list = new List<TopicEntity>();
        var testedTopic = new TopicEntity()
        {
            Id = "678b8197214ccab81d09999b",
            Name = "Test 0",
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow,
        };
        await _topics.FindOneAndReplaceAsync(t => t.Id == testedTopic.Id, testedTopic, new FindOneAndReplaceOptions<TopicEntity>() {IsUpsert = true});
        for (var i = 0; i <= 5; i++)
        {
            list.Add(new TopicEntity
            {
                Name = $"Test{i + 1}",
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
            });
        }

        var existingTestTopics = await _topics.Aggregate().Match(t => list.Any(l => l.Name == t.Name)).ToListAsync();
        var topicsToAdd = list.Where(t => existingTestTopics.All(l => l.Name != t.Name)).ToList();
        if (topicsToAdd.Count != 0)
        {
            await _topics.InsertManyAsync(topicsToAdd);
        }
    }

    public async Task<List<TopicEntity>> GetAll()
    {
        return await _topics.Aggregate().ToListAsync();
    }

    public async Task<TopicEntity> GetById(string id)
    {
        return await _topics.Find(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<TopicEntity> Create(CreateTopicRequestModel createTopicRequestModel)
    {
        var topicToCreate = new TopicEntity()
        {
            Name = createTopicRequestModel.Name,
            Description = createTopicRequestModel.Description,
            AdditionalDescription = createTopicRequestModel.AdditionalDescription,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };
        await _topics.InsertOneAsync(topicToCreate);
        return await GetById(topicToCreate.Id);
    }
}