namespace Orders.Core.Domain.Orders.Models
{
    public class OrderCreateResponse
    {
        public required string Id { get; set; }
        public required string Code { get; set; }
        public required string Customer { get; set; }
        public decimal TotalValue { get; set; }
        public List<OrderItemCreateResponse> Items { get; set; } = new List<OrderItemCreateResponse>();
    }
}