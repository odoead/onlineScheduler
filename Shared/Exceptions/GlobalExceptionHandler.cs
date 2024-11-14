using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shared.Exceptions.custom_exceptions;
using System.Net;
using System.Text.Json;

namespace Shared.Exceptions
{
    public static class GlobalExceptionHandler
    {

        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var error = contextFeature.Error;
                        var statusCode = GetStatusCode(error);
                        context.Response.StatusCode = statusCode;

                        //logger error

                        var response = new ApiErrorResponse
                        {
                            StatusCode = statusCode,
                            Message = GetErrorMessage(error, statusCode),
                            Details = error.StackTrace
                        };

                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
                    }
                });
            });
        }

        private static int GetStatusCode(Exception exception) => exception switch
        {
            BadRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ValidationException => StatusCodes.Status422UnprocessableEntity,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            NotImplementedException => StatusCodes.Status501NotImplemented,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string GetErrorMessage(Exception exception, int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "400",
            StatusCodes.Status404NotFound => "404",
            StatusCodes.Status422UnprocessableEntity => "422",
            StatusCodes.Status401Unauthorized => "401",
            StatusCodes.Status501NotImplemented => "501",
            _ => "500"
        } + $" Details: {exception.Message}";
    }
}

