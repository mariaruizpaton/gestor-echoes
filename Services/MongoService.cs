using MongoDB.Driver;
using Echoes.Models;
namespace Echoes.Services;

public class MongoService
{
    private readonly IMongoDatabase _database;

    public MongoService(IConfiguration config)
    {
        // 1. Leemos la cadena de conexión desde appsettings.json
        var connectionString = config.GetConnectionString("MongoDb");
        var client = new MongoClient(connectionString);

        // 2. Definimos el nombre de la base de datos
        _database = client.GetDatabase("EchoesDB");
    }

    // --- COLECCIONES (Equivalente a las Tablas en SQL) ---

    // Colección de Usuarios: Username es el ID único
    public IMongoCollection<Usuario> Usuarios =>
        _database.GetCollection<Usuario>("Usuarios");

    // Colección de Echoes: Posts de la red social
    public IMongoCollection<Echo> Echoes =>
        _database.GetCollection<Echo>("Echoes");

    // --- MÉTODOS DE APOYO (Opcional pero recomendado) ---

    // Ejemplo: Método rápido para verificar si un usuario existe
    public async Task<bool> ExisteUsuario(string username)
    {
        var count = await Usuarios.CountDocumentsAsync(u => u.Username == username);
        return count > 0;
    }
}
