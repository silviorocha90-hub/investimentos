using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Investimentos.Api.ExceptionHandling
{
    public class ApiExceptionHandler
        : IExceptionHandler
    {
        private readonly ILogger<ApiExceptionHandler> _logger;

        public ApiExceptionHandler(
            ILogger<ApiExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var statusCode =
                exception switch
                {
                    ArgumentException =>
                        StatusCodes.Status400BadRequest,

                    InvalidOperationException =>
                        StatusCodes.Status400BadRequest,

                    _ =>
                        StatusCodes.Status500InternalServerError
                };

            if (statusCode ==
                StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Erro não tratado na API.");
            }
            else
            {
                _logger.LogWarning(
                    exception,
                    "Requisição inválida.");
            }

            var problemDetails =
                new ProblemDetails
                {
                    Status = statusCode,

                    Title =
                        statusCode ==
                        StatusCodes.Status400BadRequest
                            ? "Requisição inválida"
                            : "Erro interno",

                    Detail =
                        statusCode ==
                        StatusCodes.Status400BadRequest
                            ? exception.Message
                            : "Ocorreu um erro interno no servidor."
                };

            httpContext.Response.StatusCode =
                statusCode;

            await httpContext.Response
                .WriteAsJsonAsync(
                    problemDetails,
                    cancellationToken);

            return true;
        }
    }
}