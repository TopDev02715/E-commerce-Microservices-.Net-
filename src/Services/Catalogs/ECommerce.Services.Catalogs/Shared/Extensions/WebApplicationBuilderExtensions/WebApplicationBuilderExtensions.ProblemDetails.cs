using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Exception.Types;
using BuildingBlocks.Security;
using BuildingBlocks.Validation;
using Hellang.Middleware.ProblemDetails;
using Newtonsoft.Json;

namespace ECommerce.Services.Catalogs.Shared.Extensions.WebApplicationBuilderExtensions;

public static partial class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddCustomProblemDetails(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails(x =>
        {
            x.ShouldLogUnhandledException = (httpContext, exception, problemDetails) =>
            {
                var env = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();
                return env.IsDevelopment() || env.IsStaging();
            };

            // Control when an exception is included
            x.IncludeExceptionDetails = (ctx, _) =>
            {
                // Fetch services from HttpContext.RequestServices
                var env = ctx.RequestServices.GetRequiredService<IHostEnvironment>();
                return env.IsDevelopment() || env.IsStaging();
            };

            x.Map<ConflictException>(ex => new ProblemDetails
            {
                Title = ex.GetType().Name,
                Status = StatusCodes.Status409Conflict,
                Detail = ex.Message,
                Type = "https://somedomain/application-rule-validation-error"
            });

            // Exception will produce and returns from our FluentValidation RequestValidationBehavior
            x.Map<ValidationException>(ex => new ProblemDetails
            {
                Title = ex.GetType().Name,
                Status = StatusCodes.Status400BadRequest,
                Detail = JsonConvert.SerializeObject(ex.ValidationResultModel.Errors),
                Type = "https://somedomain/input-validation-rules-error"
            });
            x.Map<DomainException>(ex => new ProblemDetails
            {
                Title = ex.GetType().Name,
                Status = (int)ex.StatusCode,
                Detail = ex.Message,
                Type = "https://somedomain/domain-rules-error"
            });
            x.Map<ArgumentException>(ex => new ProblemDetails
            {
                Title = ex.GetType().Name,
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
                Type = "https://somedomain/argument-error"
            });
            x.Map<BadRequestException>(ex => new ProblemDetails
            {
                Title = ex.GetType().Name,
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message,
                Type = "https://somedomain/bad-request-error"
            });
            x.Map<NotFoundException>(ex => new ProblemDetails
            {
                Title = ex.GetType().Name,
                Status = (int)ex.StatusCode,
                Detail = ex.Message,
                Type = "https://somedomain/not-found-error"
            });
            x.Map<ApiException>(ex => new ProblemDetails
            {
                Title = ex.GetType().Name,
                Status = (int)ex.StatusCode,
                Detail = ex.Message,
                Type = "https://somedomain/api-server-error"
            });
            x.Map<AppException>(ex => new ProblemDetails
            {
                Title = ex.GetType().Name,
                Status = (int)ex.StatusCode,
                Detail = ex.Message,
                Type = "https://somedomain/application-error"
            });
            x.Map<ForbiddenException>(ex => new ForbiddenProblemDetails(ex.Message));
            x.Map<UnAuthorizedException>(ex => new UnauthorizedProblemDetails(ex.Message));
            x.Map<IdentityException>(ex =>
            {
                var pd = new ProblemDetails
                {
                    Status = (int)ex.StatusCode,
                    Title = ex.GetType().Name,
                    Detail = ex.Message,
                    Type = "https://somedomain/identity-error"
                };

                return pd;
            });
            x.Map<HttpResponseException>(ex =>
            {
                var pd = new ProblemDetails
                {
                    Status = (int?)ex.StatusCode,
                    Title = ex.GetType().Name,
                    Detail = ex.Message,
                    Type = "https://somedomain/http-error"
                };

                return pd;
            });
            x.Map<HttpRequestException>(ex =>
            {
                var pd = new ProblemDetails
                {
                    Status = (int?)ex.StatusCode,
                    Title = ex.GetType().Name,
                    Detail = ex.Message,
                    Type = "https://somedomain/http-error"
                };

                return pd;
            });

            x.MapToStatusCode<ArgumentNullException>(StatusCodes.Status400BadRequest);
            x.MapStatusCode = context => new StatusCodeProblemDetails(context.Response.StatusCode);
        });

        return builder;
    }
}
