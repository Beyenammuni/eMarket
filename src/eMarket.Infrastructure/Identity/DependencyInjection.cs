using eMarket.Application.Common.Interfaces;
using eMarket.Infrastructure.Messaging;
using eMarket.Infrastructure.Persistence;
using eMarket.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Infrastructure.Identity
{
    public static class DependencyInjection
    {
        public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            services.AddScoped<PublishDomainEventsInterceptor>();
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"));
            });
        }
    }
}
