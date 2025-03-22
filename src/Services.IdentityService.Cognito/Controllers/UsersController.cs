using Diploma1.IdentityService.Extensions;
using Diploma1.IdentityService.ResponseModels;
using Diploma1.IdentityService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diploma1.IdentityService.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<UserResponseModel> GetUserByCognitoId(Guid id)
    {
        return await userService.GetUserByCognitoId(id.ToString());
    }
    
    [HttpGet]
    [Authorize]
    public async Task<UserInfoResponseModel> GetCurrentUserInfo()
    {
        return await userService.GetUserInfoById(HttpContext.User.GetUserId());
    }
}