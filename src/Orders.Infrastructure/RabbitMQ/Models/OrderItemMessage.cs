namespace Orders.Infrastructure.RabbitMQ.Models
{
    public class OrderItemMessage
    {
        public required string Produto { get; set; }
        public decimal Quantidade { get; set; }
        public decimal Preco { get; set; }
    }
}