using Common.API.Exceptions;
using UserService.Application.Exceptions;

namespace UserService.API.Exceptions;

public class AppExceptionHandler : AppExceptionHandlerBase
{
    protected override (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        UserAlreadyExistsException => (StatusCodes.Status409Conflict, "Conflict"),
        InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
    };
}
