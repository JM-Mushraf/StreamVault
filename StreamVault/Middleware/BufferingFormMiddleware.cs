using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Threading.Tasks;

namespace ProjectFileStructure.Middleware
{
    /// <summary>
    /// Middleware that ensures request body buffering for multipart forms
    /// This MUST run before any middleware that tries to read the form
    /// </summary>
    public class BufferingFormMiddleware
    {
        private readonly RequestDelegate _next;

        public BufferingFormMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if this is a multipart request
            if (context.Request.ContentType?.Contains("multipart/form-data") == true)
            {
                // Get or create the form feature
                var formFeature = context.Features.Get<IFormFeature>();

                // Replace with a buffered form feature if needed
                if (formFeature == null)
                {
                    // Configure the request to use buffering for the form
                    var formOptions = context.RequestServices
                        .GetService(typeof(Microsoft.AspNetCore.Http.Features.IFormFeature)) as IFormFeature;
                }

                // Enable buffering explicitly
                context.Request.EnableBuffering();

                // Reset body to start
                if (context.Request.Body.CanSeek)
                {
                    context.Request.Body.Position = 0;
                }
            }

            await _next(context);
        }
    }
}
