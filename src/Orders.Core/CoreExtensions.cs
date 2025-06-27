using Microsoft.Extensions.DependencyInjection;
using Orders.Core.Domain.Orders.Services;
using Orders.Core.Security;

namespace Orders.Core
{
    public static class CoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizer, Authorizer>();
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}