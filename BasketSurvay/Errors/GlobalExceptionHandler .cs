using Microsoft.AspNetCore.Diagnostics;

namespace BasketSurvay.Errors
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "something went wrrong {message}", exception.Message);

            var ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "server side error",
                Type = "https://wetopi.com/http-500-internal-server-error/#what-is-the-500-internal-server-error"
            };
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(ProblemDetails, cancellationToken);

            return true;
        }
    }
}
