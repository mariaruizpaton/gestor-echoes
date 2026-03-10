using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Usuario
{
    [BsonId]
    public string Username { get; set; } // ID Único
    public string Email { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public string Bio { get; set; }
    public List<string> Siguiendo { get; set; } = new List<string>();
}

public class Echo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Autor { get; set; }
    public string Contenido { get; set; }
    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    public int Likes { get; set; } = 0;
    public List<string> Multimedia { get; set; } = new List<string>();
}