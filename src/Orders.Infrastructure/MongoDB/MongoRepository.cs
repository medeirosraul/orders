using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Orders.Core.Domain.Common;
using Orders.Core.Interfaces;

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
            // Sessões são usadas para garantir transações atômicas no MongoDB.
            // Este trecho (e nos demais métodos de operação) verifica se há uma sessão ativa na unidade de trabalho.
            // Isso permite que as operações sejam executadas na mesma sessão caso ela exista,
            // garantindo que possa ser feita uma operação de rollback em caso de erro.
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

            // Este trecho verifica se a operação de atualização foi reconhecida pelo MongoDB.
            if (!result.IsAcknowledged)
            {
                throw new Exception("Error on updating document. Operation is not acknowledged.");
            }

            // Se o número de documentos correspondentes for zero, significa que não houve atualização.
            // Isso pode ocorrer se a versão do documento foi alterada por outra operação concorrente.
            if (result.MatchedCount == 0)
            {
                var exists = _collection.AsQueryable()
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);

                if (exists != null)
                {
                    var entityName = typeof(TEntity).Name;

                    // TODO: O ideal é lançar uma exceção específica para indicar que houve um conflito de concorrência.
                    // Isso permite que o chamador saiba que a atualização falhou devido a uma condição de concorrência.
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

        public async Task<PagedResult<TEntity>> GetPagedAsync(int page, int pageSize, IQueryable<TEntity>? query = null)
        {
            query ??= AsQueryable();

            var count = await query.CountAsync();

            page = page == 0 ? 1 : page;
            pageSize = pageSize == 0 || pageSize == int.MaxValue ? count : pageSize;

            var result = new PagedResult<TEntity>()
            {
                TotalCount = count,
                Page = page,
                PageSize = pageSize
            };

            if (count == 0) return result;

            if (pageSize != count)
                query = query.Skip((page - 1) * pageSize).Take(pageSize);

            result.Data.AddRange(await query.ToListAsync());

            return result;
        }

        public async Task<TEntity?> GetFirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return await AsQueryable()
                .FirstOrDefaultAsync(predicate);
        }
    }
}