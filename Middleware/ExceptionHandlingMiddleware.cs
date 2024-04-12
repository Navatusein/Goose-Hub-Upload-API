using System.Net;
using System.Text.Json;
using UploadApi.Dtos;

namespace UploadApi.Middleware
{
    /// <summary>
    /// Global Exception Handler Middleware
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private static Serilog.ILogger Logger => Serilog.Log.ForContext<ExceptionHandlingMiddleware>();
        private readonly RequestDelegate _next;

        /// <summary>
        /// Constructor
        /// </summary>
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Method to invoke next middleware
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = context.Response;

            var errorDto = new ErrorDto();

            Logger.Error(exception, "Handle Exception");

            switch (exception)
            {
                case ApplicationException:
                    errorDto.Code = HttpStatusCode.BadRequest.ToString();
                    errorDto.Message = "Application Exception Occured, please retry after sometime.";
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case NotImplementedException:
                    errorDto.Code = HttpStatusCode.BadRequest.ToString();
                    errorDto.Message = "NotImplementedException.";
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                default:
                    errorDto.Code = HttpStatusCode.InternalServerError.ToString(); ;
                    errorDto.Message = "Internal Server Error, Please retry after sometime";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorDto));
        }
    }
}
