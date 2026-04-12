using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleException(httpContext, ex);
        }
    }

    private async Task HandleException(HttpContext httpContext, Exception ex)
    {
        _logger.LogError(
              ex,
              "Unhandled exception. Method={Method}, Path={Path}, RequestId={RequestId}",
              httpContext.Request.Method,
              httpContext.Request.Path,
              httpContext.Request.Headers["x-request-id"]
              );

        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var statusCode = MapStatusCode(ex);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? "Внутренняя ошибка сервера."
            : ex.Message;


        var error = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = ex.Message
        };

        await httpContext.Response.WriteAsJsonAsync(error);

    }

    private static int MapStatusCode(Exception ex) =>
        ex switch
        {
            ValidationException ve => StatusCodes.Status400BadRequest,
            NotFoundException nfe => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError,

        };

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status404NotFound => "NotFound",
            _ => "Internal Server Error"
        };
    
}
