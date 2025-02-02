using Services.MessagesService.S2SModels;

namespace Services.MessagesService.HttpClients;

public class IdentityServiceHttpClient(HttpClient httpClient)
{
    public async Task<UserModel> GetUserByCognitoId(string userId)
    {
        //TODO add polly
        var response = await httpClient.GetAsync($"/Users/{userId}");
        if (response.IsSuccessStatusCode)
        {
            return (await response.Content.ReadFromJsonAsync<UserModel>())!;
        }

        return null;
    }
}