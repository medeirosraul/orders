namespace Orders.Core.Domain.Orders.Models
{
    public class OrderItemResponse
    {
        public required string Id { get; set; }
        public required string OrderCode { get; set; }
        public required string Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
    }
}