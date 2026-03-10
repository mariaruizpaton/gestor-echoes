using Echoes.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Echoes.Controllers;

[ApiController]
[Route("[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly MongoService _mongoService;

    public UsuariosController(MongoService mongoService)
    {
        _mongoService = mongoService;
    }

    // Requisito 5: POST /usuarios - Registra un nuevo perfil
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] Usuario nuevoUsuario)
    {
        // Validaciones básicas de seguridad
        if (string.IsNullOrWhiteSpace(nuevoUsuario.Username))
            return BadRequest("El nombre de usuario es obligatorio.");

        try
        {
            // Limpiamos datos por seguridad
            nuevoUsuario.FechaRegistro = DateTime.UtcNow;
            nuevoUsuario.Siguiendo = new List<string>(); // Empieza sin seguir a nadie

            // Insertamos en MongoDB
            await _mongoService.Usuarios.InsertOneAsync(nuevoUsuario);

            return CreatedAtAction(nameof(Registrar), new { id = nuevoUsuario.Username }, nuevoUsuario);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // Si el [BsonId] ya existe, Mongo lanza esta excepción
            return Conflict($"El nombre de usuario '{nuevoUsuario.Username}' ya está en uso.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    // Endpoint auxiliar para verificar que se guardan bien (opcional para pruebas)
    [HttpGet("{username}")]
    public async Task<IActionResult> ObtenerPerfil(string username)
    {
        var usuario = await _mongoService.Usuarios
            .Find(u => u.Username == username)
            .FirstOrDefaultAsync();

        if (usuario == null) return NotFound("Usuario no encontrado.");
        return Ok(usuario);
    }
}
