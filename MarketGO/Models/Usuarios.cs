using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketGO.Models
{
  
    public class Usuarios
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo {  get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;    
        public string Rol { get; set; } = "Cliente";
        public DateTime FechaRegistro { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Favoritos { get; set; } = new List<string>();


    }
}