namespace Diploma1.IdentityService.ResponseModels;

public class UserInfoResponseModel : UserResponseModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Role { get; set; }
}