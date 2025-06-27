namespace Orders.Infrastructure.RabbitMQ.Models
{
    public class OrderMessage
    {
        public int CodigoPedido { get; set; }
        public int CodigoCliente { get; set; }

        public ICollection<OrderItemMessage> Itens { get; set; } = [];
    }
}