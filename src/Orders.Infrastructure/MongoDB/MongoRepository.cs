using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Orders.Core.Domain.Common;
using Orders.Core.Interfaces;
using System.Linq.Expressions;

namespace Orders.Infrastructure.MongoDB
{
    public class MongoRepository<TEntity> : IRepository<TEntity>
        where TEntity : Entity
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<TEntity> _collection;
        private readonly IUnitOfWork _unityOfWork;

        public MongoRepository(IMongoDatabase database, IUnitOfWork unityOfWork)
        {
            _database = database;
            _collection = _database.GetCollection<TEntity>(typeof(TEntity).Name);
            _unityOfWork = unityOfWork;
        }

        public Task InsertAsync(TEntity entity)
        {
            if (_unityOfWork is MongoUnitOfWork mongoUnityOfWork && mongoUnityOfWork.GetSession() is not null)
            {
                var session = mongoUnityOfWork.GetSession();
                if (session != null)
                {
                    return _collection.InsertOneAsync(session, entity);
                }
            }

            return _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(TEntity entity)
        {
            var lastVersion = entity.ModifiedAt;

            entity.ModifiedAt = DateTime.Now;

            var filter = Builders<TEntity>.Filter.And(
                Builders<TEntity>.Filter.Eq(e => e.Id, entity.Id),
                Builders<TEntity>.Filter.Eq(e => e.ModifiedAt, lastVersion)
            );

            ReplaceOneResult result = null!;

            if (_unityOfWork is MongoUnitOfWork mongoUnityOfWork && mongoUnityOfWork.GetSession() is not null)
            {
                var session = mongoUnityOfWork.GetSession();
                if (session != null)
                {
                    result = await _collection.ReplaceOneAsync(session, filter, entity, new ReplaceOptions { IsUpsert = false });
                }
            }
            else
            {
                result = await _collection.ReplaceOneAsync(filter, entity, new ReplaceOptions { IsUpsert = false });
            }

            // Concurrency check
            if (!result.IsAcknowledged)
            {
                throw new Exception("Error on updating document. Operation is not acknowledged.");
            }

            if (result.MatchedCount == 0)
            {
                var exists = _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);

                if (exists != null)
                {
                    var entityName = typeof(TEntity).Name;

                    throw new Exception($"Concurrency error on trying to update Entity {entityName}, with ID {entity.Id}.");
                }
            }
        }

        public Task DeleteAsync(string id)
        {
            if (_unityOfWork is MongoUnitOfWork mongoUnityOfWork && mongoUnityOfWork.GetSession() is not null)
            {
                var session = mongoUnityOfWork.GetSession();
                if (session != null)
                {
                    return _collection.DeleteOneAsync(session, e => e.Id == id);
                }
            }

            return _collection.DeleteOneAsync(e => e.Id == id);
        }

        public async Task<TEntity?> GetByIdAsync(string id)
        {
            return await AsQueryable()
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);
        }

        public IQueryable<TEntity> AsQueryable()
        {
            return _collection.AsQueryable();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await AsQueryable()
                .Where(x => !x.Deleted)
                .ToListAsync();
        }

        public async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await AsQueryable()
                .FirstOrDefaultAsync(predicate);
        }
    }
}