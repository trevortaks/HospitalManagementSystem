using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using HospitalManagementSystem.Application.Common.Exceptions;

namespace HospitalManagementSystem.WebAPI.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                NotFoundException => new { StatusCode = 404, Message = exception.Message },
                ValidationException => new { StatusCode = 400, Message = exception.Message },
                BadRequestException => new { StatusCode = 400, Message = exception.Message },
                ConflictException => new { StatusCode = 409, Message = exception.Message },
                UnauthorizedException => new { StatusCode = 401, Message = exception.Message },
                ForbiddenAccessException => new { StatusCode = 403, Message = exception.Message },
                _ => new { StatusCode = 500, Message = "An internal server error occurred" }
            };

            context.Response.StatusCode = response.StatusCode;

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
