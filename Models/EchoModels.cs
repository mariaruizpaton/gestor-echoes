using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Echoes.Models;

/// <summary>
/// Representa a un usuario dentro de la plataforma de microblogging Echoes.
/// Almacena información de perfil y la red de contactos (seguidos).
/// </summary>
/// <author>Liviu</author>
public class Usuario
{
    /// <summary>
    /// Nombre de usuario único que actúa como identificador principal (Clave Primaria).
    /// </summary>
    [BsonId]
    [Required]
    public string Username { get; set; }

    /// <summary>
    /// Dirección de correo electrónico del usuario. Requerido para notificaciones y seguridad.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>
    /// Fecha y hora universal (UTC) en la que el usuario creó su cuenta.
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Breve descripción biográfica del usuario. 
    /// Restringido a 160 caracteres según Requisito 3.A.
    /// </summary>
    [MaxLength(160)]
    public string Bio { get; set; }

    /// <summary>
    /// Lista de nombres de usuario (Usernames) a los que este perfil sigue actualmente.
    /// </summary>
    public List<string> Siguiendo { get; set; } = new List<string>();
}

/// <summary>
/// Representa una publicación o "Echo" en el sistema.
/// Diseñado con un esquema dinámico para soportar diversos tipos de contenido multimedia.
/// </summary>
/// <author>Tu Nombre</author>
public class Echo
{
    /// <summary>
    /// Identificador único autogenerado por MongoDB en formato BSON ObjectId.
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// Referencia al Username del usuario que realizó la publicación.
    /// </summary>
    [Required]
    public string Autor { get; set; }

    /// <summary>
    /// Texto de la publicación. 
    /// Restringido a 280 caracteres según Requisito 3.B.
    /// </summary>
    [Required]
    [MaxLength(280)]
    public string Contenido { get; set; }

    /// <summary>
    /// Fecha y hora universal (UTC) de la publicación para ordenamiento cronológico.
    /// </summary>
    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Contador de interacciones sociales (likes). Inicia en 0.
    /// </summary>
    public int Likes { get; set; } = 0;

    /// <summary>
    /// Colección de nombres de archivos multimedia asociados.
    /// Los nombres siguen el formato UUID.webp según Requisito 4.
    /// </summary>
    public List<string>? Multimedia { get; set; } = new List<string>();
}