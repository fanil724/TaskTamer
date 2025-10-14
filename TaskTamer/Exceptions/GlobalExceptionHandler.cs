using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;

namespace TaskTamer_API.Exceptions
{
    public class GlobalExceptionHandler(IHostEnvironment env)
        : IExceptionHandler
    {
        private const string UnhandledExceptionMsg = "An unhandled exception has occurred while executing the request.";
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
            CancellationToken cancellationToken)
        {
            Logger.Error(exception, 
                exception is ApplicationException ? exception.Message : UnhandledExceptionMsg);

            var (statusCode, title) = GetStatusCodeAndTitle(exception);
            context.Response.StatusCode = statusCode;

            var problemDetails = CreateProblemDetails(context, exception, statusCode, title);
            var json = ToJson(problemDetails);

            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(json, cancellationToken);

            return true;
        }

        private (int StatusCode, string Title) GetStatusCodeAndTitle(Exception exception)
        {
            return exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                ConflictException => (StatusCodes.Status409Conflict, exception.Message),
                ValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
                ArgumentNullException => (StatusCodes.Status400BadRequest, "Validation error"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Validation error"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status500InternalServerError, UnhandledExceptionMsg)
            };
        }

        private ProblemDetails CreateProblemDetails(
            HttpContext context, 
            Exception exception,
            int statusCode,
            string title)
        {
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Instance = context.Request.Path,
                Detail = env.IsDevelopment() ? exception.ToString() : null,
                Extensions =
                {
                    ["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier,
                    ["requestId"] = context.TraceIdentifier,
                    ["timestamp"] = DateTime.UtcNow.ToString("O")
                }
            };

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] = validationException.Errors;
            }

            return problemDetails;
        }

        private string ToJson(ProblemDetails problemDetails)
        {
            try
            {
                return JsonSerializer.Serialize(problemDetails, SerializerOptions);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An exception has occurred while serializing error to JSON");
                return JsonSerializer.Serialize(new
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An error occurred while processing your request."
                }, SerializerOptions);
            }
        }
    }

    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ConflictException : ApplicationException
    {
        public ConflictException(string message) : base(message) { }
    }

    public class ValidationException : ApplicationException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors) 
            : base("One or more validation errors occurred")
        {
            Errors = errors;
        }
    }
}