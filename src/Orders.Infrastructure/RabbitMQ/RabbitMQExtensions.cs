using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure.RabbitMQ.Consumers;

namespace Orders.Infrastructure.RabbitMQ
{
    public static class RabbitMQExtensions
    {
        public static IServiceCollection AddRabbitMQConsumers(this IServiceCollection services)
        {
            services.AddHostedService<OrderConsumer>();
            return services;
        }
    }
}