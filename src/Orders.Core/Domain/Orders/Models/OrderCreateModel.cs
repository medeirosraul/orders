namespace Orders.Core.Domain.Orders.Models
{
    public class OrderCreateModel
    {
        public required string Code { get; set; }
        public required string Customer { get; set; }
        public List<OrderItemCreateModel> Items { get; set; } = new List<OrderItemCreateModel>();
    }
}