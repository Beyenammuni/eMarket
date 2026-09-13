using eMarket.Api.Common.Authentication;
using eMarket.Application.Behaviors;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.Interfaces.Payments;
using eMarket.Application.Common.IRepositories;
using eMarket.Infrastructure.Identity.Jwt;
using eMarket.Infrastructure.Messaging;
using eMarket.Infrastructure.Payments;
using eMarket.Infrastructure.Subscriptions;
using eMarket.Infrastructure.Admin;
using eMarket.Infrastructure.Persistence;
using eMarket.Infrastructure.Persistence.Interceptors;
using eMarket.Infrastructure.Persistence.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace eMarket.Infrastructure.Identity
{
    public static class DependencyInjection
    {
        public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(IAssemplyMarker).Assembly);
            });
            services.AddValidatorsFromAssembly(typeof(IAssemplyMarker).Assembly);
            services.AddTransient(
            typeof(IPipelineBehavior<,>),
              typeof(ValidationBehavior<,>));

            services.AddScoped<PublishDomainEventsInterceptor>();
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IBusinessRepository, BusinessRepository>();
            services.AddScoped<ISellerDashboardRepository, SellerDashboardRepository>();
            services.AddScoped<IBusinessAuthorization,
                BusinessAuthorization>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IBusinessDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

            services.AddScoped<ICatalogDbContext>(sp =>
                sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddScoped<IIdentityDbContext>(sp =>
                sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();

            var paymentProvider = configuration["Payments:Provider"]?.Trim();

            //if (string.Equals(
            //        paymentProvider,
            //        "Iyzico",
            //        StringComparison.OrdinalIgnoreCase))
            //{
            //    //services.AddScoped<IPaymentGateway, IyzicoPaymentGateway>();
            //}
            //else
            //{
            //    throw new InvalidOperationException(
            //        "A valid payment provider must be configured.");
            //}

            if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), Environments.Production, StringComparison.OrdinalIgnoreCase))
            {
                var allowedReturnHosts = configuration.GetSection("Payments:AllowedReturnHosts").Get<string[]>() ?? [];
                if (allowedReturnHosts.Length == 0)
                    throw new InvalidOperationException(
                        "Payments:AllowedReturnHosts must be configured in production.");

                if (string.IsNullOrWhiteSpace(configuration["Payments:WebhookSecret"]))
                    throw new InvalidOperationException(
                        "Payments:WebhookSecret must be configured in production.");
            }
            services.AddScoped<ISubscriptionDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddHostedService<SubscriptionOrderBackgroundService>();

            services.AddTransient<IPaymentGateway, MockPaymentGateway>();

            services.Configure<JwtOptions>(
                configuration.GetSection(JwtOptions.SectionName));

            services.AddScoped<IJwtService, JwtService>();
            var jwtSection =
    configuration.GetSection(JwtOptions.SectionName);

            var jwtOptions =
                jwtSection.Get<JwtOptions>()
                ?? throw new InvalidOperationException(
                    "JWT configuration is missing.");

            if (string.IsNullOrWhiteSpace(configuration["Jwt:Key"]) ||
                Encoding.UTF8.GetByteCount(configuration["Jwt:Key"]) < 32)
                throw new InvalidOperationException(
                    "JWT key must be configured and contain at least 32 bytes.");

            if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
                string.IsNullOrWhiteSpace(jwtOptions.Audience))
                throw new InvalidOperationException(
                    "JWT issuer and audience are required.");

            if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
                throw new InvalidOperationException(
                    "DefaultConnection is not configured.");

            services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "Authentication is required or the access token is invalid."
                };
                problem.Extensions["errorCode"] = "Auth.Unauthorized";
                problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(
                    problem,
                    context.HttpContext.RequestAborted);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = "You do not have permission to perform this operation."
                };
                problem.Extensions["errorCode"] = "Auth.Forbidden";
                problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(
                    problem,
                    context.HttpContext.RequestAborted);
            }
        };

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            configuration["Jwt:Key"])),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
    });

            services.AddAuthorization();

        }
    }

}
