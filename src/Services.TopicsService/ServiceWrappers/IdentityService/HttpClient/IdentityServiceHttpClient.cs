using Services.Topics.ServiceWrappers.IdentityService.Exceptions;

namespace Services.Topics.ServiceWrappers.IdentityService.HttpClient;

public class IdentityServiceHttpClient(System.Net.Http.HttpClient httpClient)
{
    public async Task<bool> IsUserAdmin(string userId)
    {
        //TODO add polly
        var response = await httpClient.GetAsync($"/Users/isAdmin/{userId}");
        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<bool>())!;
        }

        throw new UserNotFoundException(userId);
    }
}