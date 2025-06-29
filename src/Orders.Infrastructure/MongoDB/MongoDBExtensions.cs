using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Orders.Core.Interfaces;

namespace Orders.Infrastructure.MongoDB
{
    public static class MongoDBExtensions
    {
        public static void AddMongoDB(this IServiceCollection services, string connectionString, string databaseName)
        {
            // Configuração dos serviços para utilização do repositório MongoDB.
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            // Implementação do UnitOfWork e do repositório genérico.
            services.AddScoped<IUnitOfWork, MongoUnitOfWork>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return new MongoUnitOfWork(client, databaseName);
            });

            services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));
        }
    }
}