using Orders.Core;
using Orders.Infrastructure.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Database

var connectionString = builder.Configuration["Database:ConnectionString"];
var databaseName = builder.Configuration["Database:Name"];

builder.Services.AddMongoDB(connectionString, databaseName);

#endregion

builder.Services.AddCore();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();