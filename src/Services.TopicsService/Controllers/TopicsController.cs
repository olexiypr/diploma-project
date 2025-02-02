using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Services.Topics.Entities;
using Services.Topics.Services;

namespace Services.Topics.Controllers;

[ApiController]
[Route("[controller]")]
public class TopicsController(ITopicsService topicsService) : ControllerBase
{
    [HttpGet]
    public async Task<List<TopicEntity>> GetAll()
    {
        return await topicsService.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<TopicEntity> GetById(string id)
    {
        var rgx = new Regex("^[a-fA-F0-9]{24}$");
        var match = rgx.Match(id);
        if (match.Success)
        {
            
        }
        return await topicsService.GetById(id);
    }

    [HttpPost]
    public async Task<TopicEntity> Create([FromBody] string title)
    {
        return await topicsService.Create(title);
    }
}