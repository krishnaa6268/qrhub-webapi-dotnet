using System.Net;

namespace QRHub.Application.Common.Exceptions;

public abstract class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public List<string>? Errors { get; }

    protected ApiException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, List<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message) 
        : base(message, HttpStatusCode.NotFound) { }
}

public class BadRequestException : ApiException
{
    public BadRequestException(string message, List<string>? errors = null) 
        : base(message, HttpStatusCode.BadRequest, errors) { }
}

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message = "Invalid email or password.") 
        : base(message, HttpStatusCode.Unauthorized) { }
}

public class ConflictException : ApiException
{
    public ConflictException(string message) 
        : base(message, HttpStatusCode.Conflict) { }
}

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message = "You do not have permission to access this resource.") 
        : base(message, HttpStatusCode.Forbidden) { }
}
