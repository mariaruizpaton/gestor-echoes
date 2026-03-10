using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

public class Usuario
{
    [BsonId]
    [Required]
    public string Username { get; set; } // Identificador principal (Único por naturaleza en [BsonId])

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    [MaxLength(160)] // Requisito 3.A: Máximo 160 caracteres
    public string Bio { get; set; }

    public List<string> Siguiendo { get; set; } = new List<string>();
}

public class Echo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } // Autogenerado por MongoDB

    [Required]
    public string Autor { get; set; } // Referencia al Username

    [Required]
    [MaxLength(280)] // Requisito 3.B: Máximo 280 caracteres
    public string Contenido { get; set; }

    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

    public int Likes { get; set; } = 0;

    // Guardará strings tipo "550e8400-e29b-41d4-a716-446655440000.webp"
    public List<string>? Multimedia { get; set; } = new List<string>();
}