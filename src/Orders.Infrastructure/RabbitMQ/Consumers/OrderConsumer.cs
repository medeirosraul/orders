using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Orders.Models;
using Orders.Core.Domain.Orders.Services;
using Orders.Core.Extensions;
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
        private readonly IConfiguration _configuration;

        public OrderConsumer(ILogger<OrderConsumer> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory();

            // As configurações do RabbitMQ são obtidas do arquivo de configuração ou variáveis de ambiente,
            // utilizando o método de extensão Require para garantir que as chaves existam.
            factory.HostName = _configuration.Require("RabbitMQ:HostName");
            factory.Port = Convert.ToInt32(_configuration.Require("RabbitMQ:Port"));
            factory.VirtualHost = _configuration.Require("RabbitMQ:VirtualHost");
            factory.UserName = _configuration.Require("RabbitMQ:UserName");
            factory.Password = _configuration.Require("RabbitMQ:Password");

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueBindAsync(queue: "orders.created.service", exchange: "orders.events.topic", routingKey: "orders.created");

            _logger.LogInformation("RabbitMQ initialized.");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += OnReceivedAsync;

            await channel.BasicConsumeAsync(queue: "orders.created.service", autoAck: false, consumer: consumer);

            // Mantém o serviço em execução até que seja solicitado o cancelamento.
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
                // Confirma o processamento da mensagem para que seja removida da fila.
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

            // Deserializa a mensagem recebida para o modelo OrderMessage.
            // OrderMessage é o modelo que representa a estrutura dos dados expressada na documentação.
            var orderMessage = JsonSerializer.Deserialize<OrderMessage>(message, options);

            if (orderMessage is null)
                return;

            // O modelo OrderMessage é convertido para o modelo OrderCreateModel,
            // que é utilizado pelo serviço de criação de pedidos. Assim, posso manter um padrão mais coerente.
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

            // Um novo escopo do container de dependencias é criado para resolver o serviço de criação de pedidos.
            using var scope = _serviceScopeFactory.CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            await orderService.CreateOrder(orderCreateModel);
        }
    }
}