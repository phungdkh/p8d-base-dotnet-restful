namespace P8D.Infrastructure.Mvc.Filters
{
    using System.Net;
    using P8D.Infrastructure.Mvc.ActionResults;
    using P8D.Infrastructure.Mvc.Exceptions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    /// <summary>
    ///   The handle exception.
    /// </summary>
    public partial class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<HttpGlobalExceptionFilter> logger;

        /// <summary>
        ///   Initializes a new instance of the <see cref="HttpGlobalExceptionFilter" /> class.
        /// </summary>
        /// <param name="env">The web host enviroment.</param>
        /// <param name="logger">The logger.</param>
        public HttpGlobalExceptionFilter(ILogger<HttpGlobalExceptionFilter> logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void OnException(ExceptionContext context)
        {
            this.logger.LogError(
                new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.Message);

            if (context.Exception.GetType() == typeof(BaseDomainException))
            {
                var json = new JsonErrorResponse
                {
                    Messages = new[] { context.Exception.Message },
                };

                context.Result = new BadRequestObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                var json = new JsonErrorResponse
                {
                    Messages = new[] { "An error occurred. Try it again." },
                };

                json.DeveloperMessage = context.Exception;

                context.Result = new InternalServerErrorObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }

            context.ExceptionHandled = true;
        }
    }
}
