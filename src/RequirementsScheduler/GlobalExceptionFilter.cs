using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RequirementsScheduler
{
    public sealed class CustomExceptionHandlerMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;

        public CustomExceptionHandlerMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<CustomExceptionHandlerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                try
                {
                    // Do custom stuff
                    // Could be just as simple as calling _logger.LogError

                    // if you don't want to rethrow the original exception
                    // then call return:
                    // return;
                }
                catch (Exception ex2)
                {
                    _logger.LogError(
                        0, ex2,
                        "An exception was thrown attempting " +
                        "to execute the error handler.");
                }

                // Otherwise this handler will
                // re -throw the original exception
                throw;
            }
        }
    }
}