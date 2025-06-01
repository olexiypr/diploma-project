namespace Diploma1.Common.Exceptions.NotFoundExceptions;

public class UserNotFoundException(string id) : NotFoundException("User", id)
{
    
}