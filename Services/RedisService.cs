using StackExchange.Redis;
using System.Text.Json;

namespace Echoes.Services;

/// <summary>
/// Orquestador del Motor de Caché y Sesiones (Clave-Valor en RAM).
/// Implementa la gestión de TTL para datos efímeros.
/// </summary>
/// <author>Maria</author>
public class RedisService
{
    private readonly IDatabase _cache;
    private readonly IConnectionMultiplexer _redis;

    public RedisService(IConfiguration config)
    {
        _redis = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis"));
        _cache = _redis.GetDatabase();
    }

    /// <summary>
    /// Persiste una sesión en RAM con un tiempo de vida (TTL) de 30 minutos (Requisito 2).
    /// </summary>
    public async Task CrearSesionAsync(string username, string token)
    {
        TimeSpan expiry = TimeSpan.FromMinutes(30);
        await _cache.StringSetAsync($"session:{token}", username, expiry);
    }

    public async Task<string> ObtenerUsuarioPorSesionAsync(string token)
    {
        return await _cache.StringGetAsync($"session:{token}");
    }

    /// <summary>
    /// Almacena el Timeline en caché para optimizar lecturas (Requisito 5).
    /// </summary>
    public async Task CachearTimelineAsync<T>(string username, T datos)
    {
        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        string json = JsonSerializer.Serialize(datos, opciones);
        await _cache.StringSetAsync($"timeline:{username}", json, TimeSpan.FromMinutes(5));
    }

    public async Task<T?> ObtenerTimelineCacheadoAsync<T>(string username)
    {
        var json = await _cache.StringGetAsync($"timeline:{username}");
        if (json.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(json!);
    }
}