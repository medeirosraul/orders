using Microsoft.Extensions.DependencyInjection;
using Orders.Core.Domain.Orders.Services;

namespace Orders.Core
{
    public static class CoreExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}