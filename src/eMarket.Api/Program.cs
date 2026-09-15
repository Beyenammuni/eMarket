using System.Threading.RateLimiting;
using eMarket.Api.Common.Business;
using eMarket.Api.Endpoints.Admin;
using eMarket.Api.Endpoints.Catalog.Categories;
using eMarket.Api.Endpoints.Catalog.Products;
using eMarket.Api.Endpoints.Identity;
using eMarket.Api.Endpoints.Orders;
using eMarket.Api.Endpoints.Payments;
using eMarket.Api.Endpoints.Sales.Carts;
using eMarket.Api.Endpoints.Subscriptions;
using eMarket.Infrastructure.Identity;
using eMarket.SharedKernel.Exceptions;
using eMarket.Api.Common;
using eMarket.Infrastructure.Health;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using eMarket.Application.Catalog.Categories.Commands.UpdateCategory;
using eMarket.Api.Endpoints.Seller;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddResponseCompression();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length == 0)
            return;

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 120),
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue("RateLimiting:AuthPermitLimit", 10),
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:AuthWindowSeconds", 60)),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        await ApiResults.WriteProblemAsync(
            context.HttpContext.Response,
            StatusCodes.Status429TooManyRequests,
            "Too Many Requests",
            "Please retry later.",
            "RateLimit.Exceeded",
            cancellationToken);
    };
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {your JWT token}"
        });

    options.AddSecurityRequirement(document =>
    {
        var schemeReference = new OpenApiSecuritySchemeReference("Bearer", document);
        return new OpenApiSecurityRequirement { [schemeReference] = [] };
    });
});


builder.Services.AddScoped<IBusinessContext, BusinessContext>();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddScoped<AdminSeeder>();
var app = builder.Build();

app.UseExceptionHandler();

var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

if (app.Configuration.GetValue("ForwardedHeaders:TrustAllProxies", true))
{
    forwardedHeaders.KnownNetworks.Clear();
    forwardedHeaders.KnownProxies.Clear();
}

app.UseForwardedHeaders(forwardedHeaders);

app.UseResponseCompression();
if (app.Configuration.GetValue("Security:UseHttpsRedirection", app.Environment.IsDevelopment()))
    app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseRateLimiter();

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers.XXSSProtection = "0";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";

    if (!context.Request.Path.StartsWithSegments("/swagger"))
    {
        context.Response.Headers.ContentSecurityPolicy =
            "default-src 'none'; frame-ancestors 'none'";
    }

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapSwagger();
    app.MapSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();
app.MapAssignSellerRoleEndpoint();
app.MapCartEndpoints();
app.MapCategoryEndpoints();
app.MapProductEndpoints();
app.MapBusinessEndpoints();
app.MapOrderEndpoints();
app.MapPaymentEndpoints();
app.MapGetSellerDashboardEndpoint();
app.MapAuthEndpoints();
app.MapSubscriptionEndpoints();
app.MapAdminEndpoints();
app.MapApproveBusinessEndpoint();
app.MapCloseBusinessEndpoint();
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider
        .GetRequiredService<AdminSeeder>();

    await seeder.SeedAsync();
}
app.Run();

static string GetClientKey(HttpContext context)
    => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

public partial class Program;
