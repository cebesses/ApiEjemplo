using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

var cadena = builder.Configuration.GetConnectionString("Tienda")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Tienda'.");

builder.Services.AddSingleton(_ => new MySqlDataSource(cadena));
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
