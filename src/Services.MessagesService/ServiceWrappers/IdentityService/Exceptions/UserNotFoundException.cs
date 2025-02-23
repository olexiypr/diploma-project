namespace Services.MessagesService.ServiceWrappers.IdentityService.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string userId) : base($"User {userId} was not found")
    {
        
    }
}