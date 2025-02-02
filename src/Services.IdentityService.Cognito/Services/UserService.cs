using Diploma1.IdentityService.Entities;
using Diploma1.IdentityService.Exceptions;
using Diploma1.IdentityService.ResponseModels;
using Microsoft.EntityFrameworkCore;

namespace Diploma1.IdentityService.Services;

public class UserService(IdentityServiceDbContext dbContext) : IUserService
{
    public async Task<UserResponseModel> GetUserByCognitoId(string cognitoId)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.CognitoId == cognitoId);
        if (user is null)
        {
            throw new UserNotFoundException();
        }

        return Map(user);
    }

    private UserResponseModel Map(UserEntity userEntity)
    {
        return new UserResponseModel
        {
            Id = userEntity.Id,
        };
    } 
}