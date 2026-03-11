using Echoes.Services;
using Echoes.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Echoes.Controllers;

/// <summary>
/// Controlador para la gestión de perfiles de usuario y relaciones sociales.
/// </summary>
/// <author>Maria</author>
[ApiController]
[Route("[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly MongoService _mongoService;

    public UsuariosController(MongoService mongoService)
    {
        _mongoService = mongoService;
    }

    /// <summary>
    /// Registra un nuevo perfil en el motor documental (Requisito 5).
    /// </summary>
    /// <param name="nuevoUsuario">Datos del usuario a registrar.</param>
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] Usuario nuevoUsuario)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            nuevoUsuario.FechaRegistro = DateTime.UtcNow;
            nuevoUsuario.Siguiendo = new List<string>();

            await _mongoService.Usuarios.InsertOneAsync(nuevoUsuario);
            return CreatedAtAction(nameof(ObtenerPerfil), new { username = nuevoUsuario.Username }, nuevoUsuario);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return Conflict($"El nombre de usuario '{nuevoUsuario.Username}' ya está en uso.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno: {ex.Message}");
        }
    }

    /// <summary>
    /// Recupera la información de un perfil específico.
    /// </summary>
    [HttpGet("{username}")]
    public async Task<IActionResult> ObtenerPerfil(string username)
    {
        var usuario = await _mongoService.Usuarios
            .Find(u => u.Username == username)
            .FirstOrDefaultAsync();

        if (usuario == null) return NotFound("Usuario no encontrado.");
        return Ok(usuario);
    }

    /// <summary>
    /// Establece una relación de seguimiento entre dos usuarios.
    /// </summary>
    [HttpPost("{username}/seguir/{targetUsername}")]
    public async Task<IActionResult> SeguirUsuario(string username, string targetUsername)
    {
        var target = await _mongoService.Usuarios.Find(u => u.Username == targetUsername).FirstOrDefaultAsync();
        if (target == null) return NotFound("El usuario que intentas seguir no existe.");

        var filter = Builders<Usuario>.Filter.Eq(u => u.Username, username);
        var update = Builders<Usuario>.Update.AddToSet(u => u.Siguiendo, targetUsername);

        var result = await _mongoService.Usuarios.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0) return NotFound("Usuario origen no encontrado.");
        return Ok($"Ahora sigues a {targetUsername}");
    }
}