using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SV.Store.Abstractions;
using System.Threading.Tasks;

namespace ProjectFileStructure.Controllers.Middleware
{
    public class ExceptionMiddlewareImpl
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddlewareImpl> _logger;

        public ExceptionMiddlewareImpl(RequestDelegate next, ILogger<ExceptionMiddlewareImpl> logger)
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
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
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { success = false, message = "An error occurred." }));
            }
        }
    }
}