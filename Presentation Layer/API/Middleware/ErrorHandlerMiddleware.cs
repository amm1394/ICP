using Shared.Icp.Exceptions;
using System.Net;
using System.Text.Json;

namespace Presentation.Icp.API.Middleware
{
    /// <summary>
    /// Centralized exception handling middleware for the API.
    /// </summary>
    /// <remarks>
    /// This middleware intercepts unhandled exceptions thrown during the HTTP request pipeline,
    /// logs them, maps known exception types to appropriate HTTP status codes, and returns a
    /// consistent JSON error response body.
    /// 
    /// Mapping rules:
    /// - NotFoundException → 404 Not Found
    /// - ValidationException → 400 Bad Request (with field-level <see cref="ErrorResponse.Errors"/>)
    /// - BusinessRuleException → 400 Bad Request
    /// - FileProcessingException → 400 Bad Request
    /// - Any other exception → 500 Internal Server Error (Persian default message)
    /// </remarks>
    public class ErrorHandlerMiddleware
    {
        /// <summary>
        /// The next middleware component in the HTTP request pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// The logger used to record exception and pipeline diagnostic information.
        /// </summary>
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorHandlerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware delegate in the HTTP request pipeline.</param>
        /// <param name="logger">The logger used for recording exception details.</param>
        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes the current HTTP request and handles any unhandled exceptions by producing
        /// a standardized JSON error response.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// User-facing error messages returned in the JSON payload remain localized in Persian where applicable.
        /// </remarks>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Maps the given <paramref name="exception"/> to an HTTP status code and a JSON error payload,
        /// and writes it to the response stream.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        /// <remarks>
        /// The default message for unknown errors is returned in Persian to be user-friendly: "خطای سرور رخ داده است".
        /// Validation errors populate the <see cref="ErrorResponse.Errors"/> dictionary.
        /// </remarks>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Success = false,
                Message = exception.Message
            };

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = notFoundEx.Message;
                    break;

                case ValidationException validationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = validationEx.Message;
                    response.Errors = validationEx.Errors;
                    break;

                case BusinessRuleException businessEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = businessEx.Message;
                    break;

                case FileProcessingException fileEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = fileEx.Message;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "خطای سرور رخ داده است";
                    break;
            }

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}