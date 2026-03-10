using StackExchange.Redis;
using System.Text.Json;

namespace Echoes.Services;

public class RedisService
{
    private readonly IDatabase _cache;
    private readonly IConnectionMultiplexer _redis;

    public RedisService(IConfiguration config)
    {
        // Conectamos al servidor de Redis (ej: "localhost:6379")
        _redis = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis"));
        _cache = _redis.GetDatabase();
    }

    // --- GESTIÓN DE SESIONES (Requisito 2 y 5) ---
    public async Task CrearSesionAsync(string username, string token)
    {
        // Guardamos el token con un TTL nativo de 30 minutos
        TimeSpan expiry = TimeSpan.FromMinutes(30);
        await _cache.StringSetAsync($"session:{token}", username, expiry);
    }

    public async Task<string> ObtenerUsuarioPorSesionAsync(string token)
    {
        return await _cache.StringGetAsync($"session:{token}");
    }

    // --- GESTIÓN DE CACHÉ DEL TIMELINE (Requisito 5) ---
    public async Task CachearTimelineAsync<T>(string username, T datos)
    {
        var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        string json = JsonSerializer.Serialize(datos, opciones);

        // El timeline puede expirar en 5 o 10 minutos para forzar refresco
        await _cache.StringSetAsync($"timeline:{username}", json, TimeSpan.FromMinutes(5));
    }

    public async Task<T?> ObtenerTimelineCacheadoAsync<T>(string username)
    {
        var json = await _cache.StringGetAsync($"timeline:{username}");
        if (json.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>(json!);
    }
}
