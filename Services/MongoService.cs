using MongoDB.Driver;
using Echoes.Models;
namespace Echoes.Services;

/// <summary>
/// Orquestador del Motor Principal (Orientado a Documentos).
/// Maneja el almacenamiento persistente con esquema dinámico.
/// </summary>
/// <author>Maria</author>
public class MongoService
{
    private readonly IMongoDatabase _database;

    public MongoService(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("MongoDb");
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase("EchoesDB");
    }

    /// <summary> Acceso a la colección documental de Usuarios. </summary>
    public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("Usuarios");

    /// <summary> Acceso a la colección documental de Echoes. </summary>
    public IMongoCollection<Echo> Echoes => _database.GetCollection<Echo>("Echoes");

    /// <summary> Valida la existencia de un usuario de forma asíncrona. </summary>
    public async Task<bool> ExisteUsuario(string username)
    {
        var count = await Usuarios.CountDocumentsAsync(u => u.Username == username);
        return count > 0;
    }
}