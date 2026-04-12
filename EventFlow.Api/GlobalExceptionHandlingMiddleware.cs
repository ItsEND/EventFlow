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
              "Unhandled exception. Method={Method}, Path={Path}, TraceId={TraceId}",
              httpContext.Request.Method,
              httpContext.Request.Path,
              httpContext.TraceIdentifier
              );

        if (httpContext.Response.HasStarted)
        {
            _logger.LogWarning(
                "Cannot write error response because the response already started TraceId = {TraceId}", httpContext.TraceIdentifier);
            throw ex;
        }

        var (statusCode, title, detail) = MapException(ex);
        
        httpContext.Response.Clear();
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var error = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = GetTypeUri(statusCode),
            Instance = httpContext.Request.Path


        };

        await httpContext.Response.WriteAsJsonAsync(error);

    }

    private static (int statusCode, string title, string detail) MapException(Exception ex) =>
        ex switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest, "Некорректный запрос", ve.Message),
            NotFoundException nfe => (StatusCodes.Status404NotFound, "Ресурс не найден", nfe.Message),
            _ => (StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера", "Произошла непредвиденная ошибка"),

        };

    private static string GetTypeUri(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://httpstatuses.com/400",
            StatusCodes.Status404NotFound => "https://httpstatuses.com/404",
            _ => "https://httpstatuses.com/500"
        };

}
