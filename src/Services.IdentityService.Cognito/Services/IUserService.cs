using Diploma1.IdentityService.RequestModels;
using Diploma1.IdentityService.ResponseModels;

namespace Diploma1.IdentityService.Services;

public interface IUserService
{
    Task AddUserToDatabase(string cognitoId, RegisterRequestModel registerRequestModel);
    Task SetRefreshTokenToDatabase(string refreshToken, string idToken);
    Task<UserResponseModel> GetUserByCognitoId(string cognitoId);
    Task<string> GetUserCognitoIdByRefreshToken(string refreshToken);
    Task<UserInfoResponseModel> GetUserInfoById(string cognitoId);
}