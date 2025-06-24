using Orders.Core.Domain.Common;
using System.Linq.Expressions;

namespace Orders.Core.Interfaces
{
    public interface IRepository<T>
        where T : Entity
    {
        Task InsertAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(string id);

        Task<T?> GetByIdAsync(string id);

        Task<IEnumerable<T>> GetAllAsync();

        IQueryable<T> AsQueryable();

        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    }
}