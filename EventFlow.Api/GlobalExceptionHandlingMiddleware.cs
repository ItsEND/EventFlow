using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventFlow.Api;

/// <summary>
/// Middleware для глобальной обработки необработанных исключений
/// и формирования единообразного JSON-ответа об ошибке.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр middleware обработки исключений.
    /// </summary>
    /// <param name="next">Следующий делегат в конвейере обработки запроса.</param>
    /// <param name="logger">Логгер для записи информации об ошибках.</param>
    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Выполняет обработку HTTP-запроса и перехватывает необработанные исключения.
    /// </summary>
    /// <param name="httpContext">Контекст HTTP-запроса.</param>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
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

    /// <summary>
    /// Формирует HTTP-ответ с данными об ошибке на основе типа исключения.
    /// </summary>
    /// <param name="httpContext">Контекст HTTP-запроса.</param>
    /// <param name="ex">Обработанное исключение.</param>
    /// <returns>Задача, представляющая асинхронную операцию записи ответа.</returns>
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
            return;
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

    /// <summary>
    /// Сопоставляет тип исключения с HTTP-статусом и описанием ошибки.
    /// </summary>
    /// <param name="ex">Исключение.</param>
    /// <returns>Кортеж со статус-кодом, заголовком и деталями ошибки.</returns>
    private static (int statusCode, string title, string detail) MapException(Exception ex) =>
        ex switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest, "Некорректный запрос", ve.Message),
            NotFoundException nfe => (StatusCodes.Status404NotFound, "Ресурс не найден", nfe.Message),
            NoAvailableSeatsException nase => (StatusCodes.Status409Conflict, "Конфликт", nase.Message),
            _ => (StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера", "Произошла непредвиденная ошибка"),

        };

    /// <summary>
    /// Возвращает URI с описанием HTTP-статуса ошибки.
    /// </summary>
    /// <param name="statusCode">HTTP-статус код.</param>
    /// <returns>URI, соответствующий коду ошибки.</returns>
    private static string GetTypeUri(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://httpstatuses.com/400",
            StatusCodes.Status404NotFound => "https://httpstatuses.com/404",
            StatusCodes.Status409Conflict => "https://httpstatuses.com/409",
            _ => "https://httpstatuses.com/500"
        };
}
