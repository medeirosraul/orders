using Orders.Core.Domain.Common;

namespace Orders.Core.Domain.Orders.Entities
{
    public class OrderItem : Entity
    {
        public required string OrderCode { get; set; }
        public required string Product { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
    }
}