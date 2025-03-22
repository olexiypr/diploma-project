using System.IdentityModel.Tokens.Jwt;
using Diploma1.IdentityService.Entities;
using Diploma1.IdentityService.Exceptions;
using Diploma1.IdentityService.Extensions;
using Diploma1.IdentityService.RequestModels;
using Diploma1.IdentityService.ResponseModels;
using Diploma1.IdentityService.Settings;
using Microsoft.EntityFrameworkCore;

namespace Diploma1.IdentityService.Services;

public class UserService(IdentityServiceDbContext dbContext, JwtTokenSettingsProvider jwtTokenSettingsProvider) : IUserService
{
    public async Task AddUserToDatabase(string cognitoId, RegisterRequestModel registerRequestModel)
    {
        var userEntity = MapUser(cognitoId, registerRequestModel);
        await dbContext.AddAsync(userEntity);
        await dbContext.SaveChangesAsync();
    }

    public async Task SetRefreshTokenToDatabase(string refreshToken, string idToken)
    {
        var userId = GetUserIdFromJwtToken(idToken);
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.CognitoId == userId);
        if (user is null)
        {
            throw new InvalidOperationException($"Oh shit! We have user with id {userId} in cognito, but in our database it doesn't exist");
        }
        user.RefreshToken = refreshToken;
        await dbContext.SaveChangesAsync();
    }

    private string GetUserIdFromJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, jwtTokenSettingsProvider.GetCognitoTokenValidationParams(), out _);
        return principal.GetUserId();
    }

    public async Task<UserResponseModel> GetUserByCognitoId(string cognitoId)
    {
        var user = await GetUserByCognitoIdOrThrowExIfNotExists(cognitoId);
        return MapToUserResponseModel(user);
    }

    public async Task<string> GetUserCognitoIdByRefreshToken(string refreshToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user is null)
        {
            throw new UserNotFoundException();
        }
        return user.CognitoId;
    }

    public async Task<UserInfoResponseModel> GetUserInfoById(string cognitoId)
    {
        var user = await GetUserByCognitoIdOrThrowExIfNotExists(cognitoId);
        return MapToUserInfoResponseModel(user);
    }

    private UserResponseModel MapToUserResponseModel(UserEntity userEntity)
    {
        return new UserResponseModel
        {
            Id = userEntity.Id,
        };
    }
    
    private UserInfoResponseModel MapToUserInfoResponseModel(UserEntity userEntity)
    {
        return new UserInfoResponseModel
        {
            Id = userEntity.Id,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
        };
    }
    
    private static UserEntity MapUser(string cognitoUserName, RegisterRequestModel requestModel)
    {
        return new UserEntity
        {
            FirstName = requestModel.FirstName,
            LastName = requestModel.LastName,
            CognitoId = cognitoUserName,
            Email = requestModel.Email,
        };
    }

    private async Task<UserEntity> GetUserByCognitoIdOrThrowExIfNotExists(string cognitoId)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.CognitoId == cognitoId);
        if (user is null)
        {
            throw new UserNotFoundException();
        }

        return user;
    }
}