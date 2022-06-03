using System.Reflection;
using BuildingBlocks.Core;
using BuildingBlocks.Logging;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Security;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serilog;
using Serilog.Events;
using ECommerce.Services.Catalogs;
using ECommerce.Services.Catalogs.Api.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Catalogs.Api.Extensions.ServiceCollectionExtensions;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
// https://benfoster.io/blog/mvc-to-minimal-apis-aspnet-6/
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider((env, c) =>
{
    // Handling Captive Dependency Problem
    // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
    // https://levelup.gitconnected.com/top-misconceptions-about-dependency-injection-in-asp-net-core-c6a7afd14eb4
    // https://blog.ploeh.dk/2014/06/02/captive-dependency/
    if (env.HostingEnvironment.IsDevelopment() || env.HostingEnvironment.IsEnvironment("tests") ||
        env.HostingEnvironment.IsStaging())
    {
        c.ValidateScopes = true;
    }
});

builder.Services.AddControllers(options =>
        options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())))
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddApplicationOptions(builder.Configuration);
var loggingOptions = builder.Configuration.GetSection(nameof(LoggerOptions)).Get<LoggerOptions>();

builder.AddCompression();
builder.AddCustomProblemDetails();

builder.Host.AddCustomSerilog(
    optionsBuilder =>
    {
        optionsBuilder
            .SetLevel(LogEventLevel.Information);
    },
    config =>
    {
        config.WriteTo.File(
            ECommerce.Services.Catalogs.Api.Program.GetLogPath(builder.Environment, loggingOptions) ?? "../logs/customers-service.log",
            outputTemplate: loggingOptions?.LogTemplate ??
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true);
    });

builder.AddCustomSwagger(builder.Configuration, typeof(CatalogRoot).Assembly);

builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddCustomJwtAuthentication(builder.Configuration);
builder.Services.AddCustomAuthorization(
    rolePolicies: new List<RolePolicy>
    {
        new(CatalogConstants.Role.Admin, new List<string> { CatalogConstants.Role.Admin }),
        new(CatalogConstants.Role.User, new List<string> { CatalogConstants.Role.User })
    });

/*----------------- Module Services Setup ------------------*/
builder.AddModulesServices();

var app = builder.Build();

var environment = app.Environment;

if (environment.IsDevelopment() || environment.IsEnvironment("docker"))
{
    app.UseDeveloperExceptionPage();

    // Minimal Api not supported versioning in .net 6
    app.UseCustomSwagger();

    // ref: https://christian-schou.dk/how-to-make-api-documentation-using-swagger/
    app.UseReDoc(options =>
    {
        options.DocumentTitle = "Catalogs Service ReDoc";
        options.SpecUrl = "/swagger/v1/swagger.json";
    });
}

app.UseProblemDetails();

app.UseSerilogRequestLogging();

app.UseRouting();
app.UseAppCors();

app.UseAuthentication();
app.UseAuthorization();

/*----------------- Module Middleware Setup ------------------*/
await app.ConfigureModules();


app.MapControllers();

/*----------------- Module Routes Setup ------------------*/
app.MapModulesEndpoints();

// automatic discover minimal endpoints
app.MapEndpoints();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

await app.RunAsync();

namespace ECommerce.Services.Catalogs.Api
{
    public partial class Program
    {
        public static string? GetLogPath(IWebHostEnvironment env, LoggerOptions loggerOptions)
            => env.IsDevelopment() ? loggerOptions.DevelopmentLogPath : loggerOptions.ProductionLogPath;
    }
}
