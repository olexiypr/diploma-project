using Amazon.CognitoIdentityProvider.Model;
using Diploma1.IdentityService.RequestModels;

namespace Diploma1.IdentityService.Services;

public interface IAuthService
{
    public Task<AuthenticationResultType> Login(LoginRequestModel loginRequestModel);
    public Task<bool> Register(RegistrerRequestModel registerRequestModel);
    Task<bool> ConfirmConfirmationCode(ConfirmConfirmationCodeRequestModel requestModel);
}