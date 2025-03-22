namespace Diploma1.IdentityService.RequestModels;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
    public string CognitoId { get; set; }
}