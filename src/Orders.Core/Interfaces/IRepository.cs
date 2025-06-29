using Orders.Core.Domain.Common;
using System.Linq.Expressions;

namespace Orders.Core.Interfaces
{
    /// <summary>
    /// Repositório genérico para entidades do tipo <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade.</typeparam>
    public interface IRepository<T>
        where T : Entity
    {
        Task InsertAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(string id);

        Task<T?> GetByIdAsync(string id);

        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> GetAllAsync();

        Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, IQueryable<T>? query = null);

        IQueryable<T> AsQueryable();
    }
}