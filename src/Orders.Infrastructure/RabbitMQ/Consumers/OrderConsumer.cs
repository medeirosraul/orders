using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Orders.Models;
using Orders.Core.Domain.Orders.Services;
using Orders.Infrastructure.RabbitMQ.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Orders.Infrastructure.RabbitMQ.Consumers
{
    public class OrderConsumer : BackgroundService
    {
        private readonly ILogger<OrderConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public OrderConsumer(ILogger<OrderConsumer> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory();

            factory.HostName = "jaragua-01.lmq.cloudamqp.com";
            factory.Port = 5672;
            factory.VirtualHost = "wiqeasaw";
            factory.UserName = "wiqeasaw";
            factory.Password = "-";

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueBindAsync(queue: "orders.created.service", exchange: "orders.events.topic", routingKey: "orders.created");

            _logger.LogInformation("RabbitMQ initialized.");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += OnReceivedAsync;

            await channel.BasicConsumeAsync(queue: "orders.created.service", autoAck: false, consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task OnReceivedAsync(object model, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation("New message received: {Message}", message);

            try
            {
                await ProcessOrderAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);
            }
            finally
            {
                // Confirma o processamento da mensagem
                var consumer = (AsyncEventingBasicConsumer)model;
                await consumer.Channel.BasicAckAsync(args.DeliveryTag, multiple: false);
            }
        }

        private async Task ProcessOrderAsync(string message)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var orderMessage = JsonSerializer.Deserialize<OrderMessage>(message, options);

            if (orderMessage is null)
                return;

            var orderCreateModel = new OrderCreateModel
            {
                Code = orderMessage.CodigoPedido.ToString(),
                Customer = orderMessage.CodigoCliente.ToString(),
                Items = orderMessage.Itens.Select(item => new OrderItemCreateModel
                {
                    Product = item.Produto,
                    Quantity = item.Quantidade,
                    UnitPrice = item.Preco
                }).ToList()
            };

            using var scope = _serviceScopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            await orderService.CreateOrder(orderCreateModel);
        }
    }
}