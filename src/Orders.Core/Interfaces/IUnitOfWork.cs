namespace Orders.Core.Interfaces
{
    /// <summary>
    /// Unidade de trabalho (Unit of Work) para gerenciar transações.
    /// </summary>
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();

        Task CommitAsync();

        Task RollbackAsync();
    }
}