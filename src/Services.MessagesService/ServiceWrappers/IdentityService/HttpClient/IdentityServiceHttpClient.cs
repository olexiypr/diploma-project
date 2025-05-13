using Services.MessagesService.ServiceWrappers.IdentityService.Exceptions;
using Services.MessagesService.ServiceWrappers.IdentityService.Models;

namespace Services.MessagesService.ServiceWrappers.IdentityService.HttpClient;

public class IdentityServiceHttpClient(System.Net.Http.HttpClient httpClient)
{
    public async Task<UserModel> GetUserByCognitoId(string userId)
    {
        //TODO add polly
        var response = await httpClient.GetAsync($"/Users/{userId}");
        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<UserModel>())!;
        }

        throw new UserNotFoundException(userId);
    }
}