using Services.MessagesService.ServiceWrappers.IdentityService.Exceptions;
using Services.MessagesService.ServiceWrappers.TopicsService.Exceptions;
using Services.MessagesService.ServiceWrappers.TopicsService.Models;

namespace Services.MessagesService.ServiceWrappers.TopicsService.HttpClient;

public class TopicsServiceHttpClient(System.Net.Http.HttpClient httpClient)
{
    public async Task<TopicModel> GetTopicById(string topicId)
    {
        //TODO add polly
        var response = await httpClient.GetAsync($"/Topics/{topicId}");
        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<TopicModel>())!;
        }

        throw new TopicNotFoundException(topicId);
    }
}