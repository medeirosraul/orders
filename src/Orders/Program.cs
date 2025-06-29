using Orders.Core;
using Orders.Core.Extensions;
using Orders.Infrastructure.MongoDB;
using Orders.Infrastructure.RabbitMQ;
using Orders.Middlewares;

var builder = WebApplication.CreateBuilder(args);

#region Database

// Buscando as configura��es do banco de dados no arquivo appsettings.json ou vari�veis de ambiente
// utilizando o m�todo de extens�o Require para garantir que as chaves existam.
var connectionString = builder.Configuration.Require("Database:ConnectionString");
var databaseName = builder.Configuration.Require("Database:Name");

// Registrando os servi�os do MongoDB e RabbitMQ
// Os servi�os s�o configurados no projeto Orders.Infrastructure.
builder.Services.AddMongoDB(connectionString, databaseName);
builder.Services.AddRabbitMQConsumers();

#endregion

// Para uma melhor separa��o de responsabilidades, os modelos e servi�os do dom�nio
// s�o implementados no projeto Orders.Core.
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

// Registrando um middleware de autoriza��o personalizado
app.UseMiddleware<AuthorizationMiddleware>();

app.MapControllers();

app.Run();