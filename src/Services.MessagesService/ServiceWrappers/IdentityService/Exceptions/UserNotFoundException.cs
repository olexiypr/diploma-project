namespace Services.MessagesService.ServiceWrappers.IdentityService.Exceptions;

public class UserNotFoundException(string userId) : Exception($"User {userId} was not found");