using eMarket.Api.Common.Authentication;
using eMarket.Application.Behaviors;
using eMarket.Application.Catalog.Categories.Commands.CreateCategory;
using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Infrastructure.Messaging;
using eMarket.Infrastructure.Persistence;
using eMarket.Infrastructure.Persistence.Interceptors;
using eMarket.Infrastructure.Persistence.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace eMarket.Infrastructure.Identity
{
    public static class DependencyInjection
    {
        public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateCategoryCommand).Assembly);
            });
            services.AddValidatorsFromAssembly(typeof(CreateCategoryCommand).Assembly);
            services.AddTransient(
            typeof(IPipelineBehavior<,>),
              typeof(ValidationBehavior<,>));

            services.AddScoped<PublishDomainEventsInterceptor>();
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBusinessRepository, BusinessRepository>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IBusinessDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

            services.AddScoped<ICatalogDbContext>(sp =>
                sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddScoped<IIdentityDbContext>(sp =>
                sp.GetRequiredService<AppDbContext>());
        }
    }
}
