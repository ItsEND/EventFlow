using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PlatformTemplate.BuildingBlocks.Api;

internal sealed class PlatformExceptionHandler(ILogger<PlatformExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Необработанная ошибка при выполнении {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Внутренняя ошибка сервера",
                Detail = "При обработке запроса произошла непредвиденная ошибка.",
                Type = "https://httpstatuses.com/500",
                Instance = httpContext.Request.Path
            },
            cancellationToken);

        return true;
    }
}
