using Services.Topics.Entities;

namespace Services.Topics.Services;

public interface ITopicsService
{
    Task<List<TopicEntity>> GetAll();
    Task<TopicEntity> GetById(string id);
    Task<TopicEntity> Create(string title);
}