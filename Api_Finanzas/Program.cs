using Api_Finanzas.Properties;
using Api_Finanzas.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Conexión a DB (ajustar si es necesario)
builder.Services.AddDbContext<FinanzasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// SOLO Swagger básico
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Controladores
builder.Services.AddControllers();

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mapeo de controladores
app.MapControllers();

app.Run();
