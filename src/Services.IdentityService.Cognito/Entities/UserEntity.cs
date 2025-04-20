namespace Diploma1.IdentityService.Entities;

public class UserEntity
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? RelativeAvatarPath { get; set; }
    public string CognitoId { get; set; }
    public string? RefreshToken { get; set; }
}