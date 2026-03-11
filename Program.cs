using Echoes.Services;
using Microsoft.Extensions.FileProviders;

/// <summary>
/// Punto de entrada principal para la API de la plataforma Echoes.
/// Configura el pipeline de ASP.NET Core, la inyección de dependencias para 
/// persistencia políglota y la gestión de recursos multimedia.
/// </summary>
/// <author>Liviu</author>
var builder = WebApplication.CreateBuilder(args);

// --- 1. Servicios de Persistencia ---

/// <summary>
/// Registro de <see cref="MongoService"/> como Singleton.
/// Maneja la persistencia documental de Usuarios y Echoes (Posts).
/// </summary>
/// <author>María</author>
builder.Services.AddSingleton<MongoService>();

/// <summary>
/// Registro de <see cref="RedisService"/> como Singleton.
/// Gestiona el almacenamiento efímero en RAM para sesiones y caché de Timeline (Requisito 2).
/// </summary>
/// <author>Marñia</author>
builder.Services.AddSingleton<RedisService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

/// <summary>
/// Configuración de Swagger para la documentación y pruebas de los Endpoints (Requisito 1).
/// </summary>
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 3. Carpeta de Multimedia (Requisito 4) ---

/// <summary>
/// Configuración del sistema de archivos para almacenamiento multimedia.
/// Las imágenes no se guardan como binarios en la BD, sino como archivos físicos .webp (Requisito 4).
/// </summary>
var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

// Asegura que el directorio de carga exista para evitar excepciones de E/S.
if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}

/// <summary>
/// Configuración de Middleware para servir archivos estáticos.
/// Mapea la ruta física 'wwwroot/uploads' a la URL pública '/multimedia'.
/// </summary>
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/multimedia"
});

// --- 4. Middleware Pipeline ---

/// <summary>
/// Configuración del entorno de desarrollo. 
/// Habilita la interfaz de Swagger y el endpoint JSON correspondiente.
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Echoes v1"));
}

/// <remarks>
/// Se recomienda precaución con HTTPS Redirection en entornos locales con NoSQL 
/// si existen conflictos de puertos con el puerto 55555.
/// </remarks>
app.UseHttpsRedirection();

app.UseAuthorization();

/// <summary>
/// Mapea las rutas de los controladores (ej: /usuarios, /echoes, /timeline).
/// </summary>
app.MapControllers();

/// <summary>
/// Arranca el servidor web Kestrel y comienza la escucha de peticiones.
/// </summary>
app.Run();