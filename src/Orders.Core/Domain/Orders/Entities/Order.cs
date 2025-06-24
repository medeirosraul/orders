using Orders.Core.Domain.Common;

namespace Orders.Core.Domain.Orders.Entities
{
    public class Order : Entity
    {
        public required string Code { get; set; }
        public required string Customer { get; set; }
        public decimal TotalValue { get; set; }
    }
}