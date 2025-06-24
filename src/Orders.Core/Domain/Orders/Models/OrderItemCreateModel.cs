namespace Orders.Core.Domain.Orders.Models
{
    public class OrderItemCreateModel
    {
        public required string Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}