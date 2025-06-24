using MongoDB.Driver;
using Orders.Core.Interfaces;

namespace Orders.Infrastructure.MongoDB
{
    public class MongoUnitOfWork : IUnitOfWork
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private IClientSessionHandle? _session;

        public MongoUnitOfWork(IMongoClient client, string databaseName)
        {
            _client = client;
            _database = _client.GetDatabase(databaseName);
        }

        public async Task BeginTransactionAsync()
        {
            if (_session != null)
                throw new InvalidOperationException("Transaction already started.");

            _session = await _client.StartSessionAsync();
            _session.StartTransaction();
        }

        public async Task CommitAsync()
        {
            if (_session == null)
                throw new InvalidOperationException("No transaction started.");

            await _session.CommitTransactionAsync();
            _session.Dispose();
            _session = null;
        }

        public async Task RollbackAsync()
        {
            if (_session == null)
                throw new InvalidOperationException("No transaction started.");

            await _session.AbortTransactionAsync();
            _session.Dispose();
            _session = null;
        }

        public IClientSessionHandle? GetSession() => _session;
    }
}