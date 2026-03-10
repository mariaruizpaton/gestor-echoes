using MongoDB.Driver;
namespace Echoes.Services;

public class MongoService
{
    private readonly IMongoDatabase _database;

    public MongoService(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDb"));
        _database = client.GetDatabase("EchoesDB");
    }

    public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("Usuarios");
    public IMongoCollection<Echo> Echoes => _database.GetCollection<Echo>("Echoes");
}
