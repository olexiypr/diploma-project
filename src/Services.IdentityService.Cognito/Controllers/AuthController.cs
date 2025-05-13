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
    public async Task<string> Register([FromBody] RegisterRequestModel registerRequest)
    {
        return await authService.Register(registerRequest);
    }

    [HttpPost("refresh")]
    public async Task<AuthenticationResultType> RefreshToken([FromBody] string refreshToken)
    {
        return await authService.RefreshToken(refreshToken);
    }


    [HttpPost("confirm")]
    public async Task<bool> ConfirmCode([FromBody] ConfirmConfirmationCodeRequestModel requestModel)
    {
        return await authService.ConfirmConfirmationCode(requestModel);
    }
}