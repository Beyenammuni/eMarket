using eMarket.Application.Common.Interfaces;
using eMarket.Infrastructure.Messaging;
using eMarket.Infrastructure.Persistence.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace eMarket.Infrastructure.Identity
{
    public class DependencyInjection
    {
        public void AddIdentityServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            services.AddScoped<PublishDomainEventsInterceptor>();
        }
    }
}
