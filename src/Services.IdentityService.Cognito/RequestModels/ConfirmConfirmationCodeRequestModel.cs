namespace Diploma1.IdentityService.RequestModels;

public class ConfirmConfirmationCodeRequestModel
{
    public string Code { get; set; }
    public string UserId { get; set; }
}