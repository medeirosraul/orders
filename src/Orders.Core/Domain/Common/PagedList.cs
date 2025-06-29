namespace Orders.Core.Domain.Common
{
    /// <summary>
    /// Lista paginada genérica.
    /// </summary>
    /// <typeparam name="T"> Tipo da lista. </typeparam>
    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> Data { get; set; } = [];
    }
}