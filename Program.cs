using GymManagementAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Vezérlők hozzáadása
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 2. Swagger / OpenAPI beállítása
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. SQLite adatbázis-kapcsolat regisztrálása
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=gym.db"));

var app = builder.Build();

// Fejlesztői környezetben a Swagger bekapcsolása
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();