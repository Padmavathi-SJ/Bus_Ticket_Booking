// BusBooking.API/Filters/GlobalExceptionFilter.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.API.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var actionName = context.ActionDescriptor.DisplayName;

            _logger.LogError(exception, "Exception in {ActionName} at {Time}", actionName, DateTime.UtcNow);

            var response = new
            {
                success = false,
                timestamp = DateTime.UtcNow,
                message = GetErrorMessage(exception),
                errorType = exception.GetType().Name
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = GetStatusCode(exception)
            };

            context.ExceptionHandled = true;
        }

        private string GetErrorMessage(Exception ex)
        {
            return ex switch
            {
                ArgumentException => ex.Message,
                ArgumentNullException => ex.Message,
                InvalidOperationException => ex.Message,
                UnauthorizedAccessException => ex.Message,
                KeyNotFoundException => "The requested resource was not found.",
                DbUpdateConcurrencyException => "Data was modified by another user. Please refresh and try again.",
                DbUpdateException => "A database error occurred. Please try again.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }

        private int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                ArgumentException => 400,
                ArgumentNullException => 400,
                InvalidOperationException => 409,
                UnauthorizedAccessException => 401,
                KeyNotFoundException => 404,
                DbUpdateException => 500,
                _ => 500
            };
        }
    }
}