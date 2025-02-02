using Amazon.CognitoIdentityProvider.Model;
using Diploma1.IdentityService.RequestModels;
using Diploma1.IdentityService.Services;
using Microsoft.AspNetCore.Mvc;

namespace Diploma1.IdentityService.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<AuthenticationResultType> Login([FromBody] LoginRequestModel loginRequest)
    {
        return await authService.Login(loginRequest);
    }

    [HttpPost("register")]
    public async Task<bool> Register([FromBody] RegistrerRequestModel registerRequest)
    {
        return await authService.Register(registerRequest);
    }


    [HttpPost("confirm")]
    public async Task<bool> ConfirmCode([FromBody] ConfirmConfirmationCodeRequestModel requestModel)
    {
        return await authService.ConfirmConfirmationCode(requestModel);
    }
}