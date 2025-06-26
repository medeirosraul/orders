namespace Orders.Core.Domain.Orders.Models
{
    public class OrderFilterModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string? Customer { get; set; }
    }
}