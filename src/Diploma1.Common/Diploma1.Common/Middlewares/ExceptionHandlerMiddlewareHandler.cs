using System.Net;
using Diploma1.Common.Exceptions;
using Diploma1.Common.Exceptions.NotFoundExceptions;

namespace Diploma1.Common.Middlewares;

public class ExceptionHandlerMiddlewareHandler
{
    public (string, int) HandleException(Exception exception)
    {
        var baseException = new DiplomaApiException();
        var statusCode = baseException.StatusCode;
        var exceptionText = baseException.Text;
        switch (exception)
        {
            case NotFoundException notFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                break;
        }
        
        throw new NotImplementedException();
    }
}