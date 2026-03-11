using Echoes.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Echoes.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly MongoService _mongo;
    private readonly RedisService _redis;

    public AuthController(MongoService mongo, RedisService redis)
    {
        _mongo = mongo;
        _redis = redis;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string username) // Simplificado para el ejercicio
    {
        // 1. Validar que el usuario existe en Mongo
        var user = await _mongo.Usuarios.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (user == null) return Unauthorized("Usuario no existe.");

        // 2. Generar un token aleatorio (simulado)
        string token = Guid.NewGuid().ToString();

        // 3. Guardar en Redis con TTL de 30 min (ya configurado en tu RedisService)
        await _redis.CrearSesionAsync(username, token);

        return Ok(new { Token = token, Message = "Sesión iniciada por 30 minutos" });
    }
}
