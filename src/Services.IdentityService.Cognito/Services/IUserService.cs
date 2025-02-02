using Diploma1.IdentityService.ResponseModels;

namespace Diploma1.IdentityService.Services;

public interface IUserService
{
    Task<UserResponseModel> GetUserByCognitoId(string cognitoId);
}