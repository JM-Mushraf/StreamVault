using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SV.Store.Abstractions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace ProjectFileStructure.Middleware
{
    public class ExceptionMiddlewareImpl
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddlewareImpl> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddlewareImpl(RequestDelegate next, ILogger<ExceptionMiddlewareImpl> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                try
                {
                    // Resolve scoped IErrorStore from the request services to avoid resolving scoped services from the root provider
                    var errorStore = context.RequestServices.GetService(typeof(SV.Store.Abstractions.IErrorStore)) as SV.Store.Abstractions.IErrorStore;
                    if (errorStore != null)
                    {
                        await errorStore.LogErrorAsync(ex.Message, ex.TargetSite?.Name ?? string.Empty, ex.HResult, ex.StackTrace ?? string.Empty);
                    }
                }
                catch { }

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                // Show detailed error in development, generic in production
                var errorMessage = _env.IsDevelopment() ? ex.Message : "An error occurred.";
                var errorDetails = _env.IsDevelopment() ? ex.StackTrace : null;

                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new 
                { 
                    success = false, 
                    message = errorMessage,
                    details = errorDetails
                }));
            }
        }
    }
}
