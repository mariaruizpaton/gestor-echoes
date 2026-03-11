using Echoes.Services;
using Echoes.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Echoes.Controllers;

/// <summary>
/// Controlador principal para la gestión de publicaciones (Echoes) y la generación del muro (Timeline).
/// Implementa una arquitectura de persistencia políglota utilizando MongoDB como almacenamiento 
/// persistente y Redis como capa de caché.
/// </summary>
/// <author>Liviu</author>
[ApiController]
[Route("[controller]")]
public class EchoesController : ControllerBase
{
    private readonly MongoService _mongo;
    private readonly RedisService _redis;

    /// <summary>
    /// Inicializa una nueva instancia del controlador inyectando los servicios NoSQL.
    /// </summary>
    /// <param name="mongo">Servicio de base de datos documental.</param>
    /// <param name="redis">Servicio de base de datos en memoria (RAM).</param>
    public EchoesController(MongoService mongo, RedisService redis)
    {
        _mongo = mongo;
        _redis = redis;
    }

    /// <summary>
    /// Publica un nuevo post aceptando contenido y archivos multimedia opcionales (Requisito 4 y 5).
    /// </summary>
    /// <param name="autor">Username del autor.</param>
    /// <param name="contenido">Texto del post (máx 280).</param>
    /// <param name="imagenes">Lista de archivos físicos (imágenes).</param>
    [HttpPost]
    public async Task<IActionResult> Publicar(
        [FromForm] string autor,
        [FromForm] string contenido,
        List<IFormFile>? imagenes) // Acepta imágenes opcionalmente
    {
        // 1. Validaciones básicas
        if (!await _mongo.ExisteUsuario(autor)) return BadRequest("Autor no existe.");
        if (contenido.Length > 280) return BadRequest("Contenido demasiado largo.");

        var nuevoEcho = new Echo
        {
            Autor = autor,
            Contenido = contenido,
            FechaPublicacion = DateTime.UtcNow,
            Multimedia = new List<string>()
        };

        // 2. PROCESAMIENTO FÍSICO (Requisito 4)
        if (imagenes != null && imagenes.Count > 0)
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            foreach (var file in imagenes)
            {
                // Generamos el nombre según el requisito: UUID + .webp
                string fileName = $"{Guid.NewGuid()}.webp";
                string fullPath = Path.Combine(rootPath, fileName);

                // Guardamos el archivo físico en el disco
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Guardamos SOLO el nombre en la base de datos
                nuevoEcho.Multimedia.Add(fileName);
            }
        }

        await _mongo.Echoes.InsertOneAsync(nuevoEcho);
        return Ok(nuevoEcho);
    }

    /// <summary>
    /// Recupera el historial de publicaciones de un usuario específico.
    /// </summary>
    /// <param name="username">Nombre de usuario a consultar.</param>
    /// <returns>Lista de publicaciones ordenadas por fecha descendente.</returns>
    [HttpGet("{username}")]
    public async Task<IActionResult> ObtenerPostsUsuario(string username)
    {
        var posts = await _mongo.Echoes
            .Find(e => e.Autor == username)
            .SortByDescending(e => e.FechaPublicacion)
            .ToListAsync();

        return Ok(posts);
    }

    /// <summary>
    /// Endpoint Crítico: Genera el muro de inicio (Timeline) del usuario.
    /// </summary>
    /// <remarks>
    /// Implementa el Requisito 5 (Cache-Aside Pattern):
    /// 1. Intenta leer el timeline desde Redis (RAM) para respuesta instantánea.
    /// 2. Si falla, consulta MongoDB, recupera los posts de los seguidos y actualiza la caché.
    /// </remarks>
    /// <param name="username">Usuario que solicita ver su muro.</param>
    /// <returns>Los últimos 10 posts de las personas a las que sigue el usuario.</returns>
    [HttpGet("/timeline/{username}")]
    public async Task<IActionResult> GetTimeline(string username)
    {
        // 1. Lectura desde Motor de Caché (Almacenamiento Efímero)
        var cache = await _redis.ObtenerTimelineCacheadoAsync<List<Echo>>(username);
        if (cache != null)
        {
            return Ok(new { Source = "Cache (Redis)", Data = cache });
        }

        // 2. Lectura desde Motor Documental (Almacenamiento Persistente)
        var usuario = await _mongo.Usuarios.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (usuario == null) return NotFound("Usuario no encontrado.");

        var seguidos = usuario.Siguiendo;
        var timeline = await _mongo.Echoes
            .Find(e => seguidos.Contains(e.Autor))
            .SortByDescending(e => e.FechaPublicacion)
            .Limit(10)
            .ToListAsync();

        // 3. Persistencia en RAM para futuras peticiones (Optimización de lectura)
        await _redis.CachearTimelineAsync(username, timeline);

        return Ok(new { Source = "Database (MongoDB)", Data = timeline });
    }
}