namespace Services.Topics.ServiceWrappers.IdentityService.Exceptions;

public class UserNotFoundException(string userId) : Exception($"User {userId} was not found");