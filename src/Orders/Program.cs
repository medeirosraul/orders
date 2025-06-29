using Orders.Core;
using Orders.Core.Extensions;
using Orders.Infrastructure.MongoDB;
using Orders.Infrastructure.RabbitMQ;
using Orders.Middlewares;

var builder = WebApplication.CreateBuilder(args);

#region Database

// Buscando as configurações do banco de dados no arquivo appsettings.json ou variáveis de ambiente
// utilizando o método de extensão Require para garantir que as chaves existam.
var connectionString = builder.Configuration.Require("Database:ConnectionString");
var databaseName = builder.Configuration.Require("Database:Name");

// Registrando os serviços do MongoDB e RabbitMQ
// Os serviços são configurados no projeto Orders.Infrastructure.
builder.Services.AddMongoDB(connectionString, databaseName);
builder.Services.AddRabbitMQConsumers();

#endregion

// Para uma melhor separação de responsabilidades, os modelos e serviços do domínio
// são implementados no projeto Orders.Core.
builder.Services.AddCore();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Registrando um middleware de autorização personalizado
app.UseMiddleware<AuthorizationMiddleware>();

app.MapControllers();

app.Run();