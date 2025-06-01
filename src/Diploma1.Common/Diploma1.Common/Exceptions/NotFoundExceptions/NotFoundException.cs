namespace Diploma1.Common.Exceptions.NotFoundExceptions;

public class NotFoundException(string entity, string param) : DiplomaApiException($"{entity} with param {param} is not found")
{
    
}