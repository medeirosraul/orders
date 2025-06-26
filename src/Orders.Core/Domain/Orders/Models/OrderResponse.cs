namespace Orders.Core.Domain.Orders.Models
{
    public class OrderResponse
    {
        public required string Id { get; set; }
        public required string Code { get; set; }
        public required string Customer { get; set; }
        public decimal TotalValue { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
    }
}