using Echoes.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Registramos nuestros servicios personalizados para MongoDB y Redis
builder.Services.AddSingleton<MongoService>();
builder.Services.AddSingleton<RedisService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();