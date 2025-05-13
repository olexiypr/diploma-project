using Amazon.CognitoIdentityProvider.Model;
using Diploma1.IdentityService.RequestModels;

namespace Diploma1.IdentityService.Services;

public interface IAuthService
{
    public Task<AuthenticationResultType> Login(LoginRequestModel loginRequestModel);
    public Task<string> Register(RegisterRequestModel registerRequestModel);
    public Task<AuthenticationResultType> RefreshToken(string refreshToken);
    Task<bool> ConfirmConfirmationCode(ConfirmConfirmationCodeRequestModel requestModel);
}