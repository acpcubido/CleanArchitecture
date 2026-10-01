using Cubido.Template.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Frozen;

namespace Cubido.Template.Web.Infrastructure;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly FrozenDictionary<Type, Func<HttpContext, Exception, Task>> exceptionHandlers;

    public CustomExceptionHandler()
    {
        // Register known exception types and handlers.
        exceptionHandlers = new Dictionary<Type, Func<HttpContext, Exception, Task>>()
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
        }.ToFrozenDictionary();
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (exceptionHandlers.TryGetValue(exceptionType, out var handler))
        {
            await handler.Invoke(httpContext, exception);
            return true;
        }

        return false;
    }

    private Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return TypedResults.ValidationProblem
        (
            errors: ((ValidationException)ex).Errors,
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        ).ExecuteAsync(httpContext);
    }

    private Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        return TypedResults.NotFound(new ProblemDetails()
        {
            Status = StatusCodes.Status404NotFound,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "The specified resource was not found.",
            Detail = ((NotFoundException)ex).Message
        }).ExecuteAsync(httpContext);
    }

    private Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    {
        return TypedResults.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Unauthorized",
            type: "https://tools.ietf.org/html/rfc7235#section-3.1"
        ).ExecuteAsync(httpContext);
    }

    private Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    {
        return TypedResults.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status403Forbidden,
            title: "Forbidden",
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        ).ExecuteAsync(httpContext);
    }
}
