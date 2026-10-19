using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketGO.Models
{
    public class Direccion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Comuna { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Departamento { get; set; }
        public string? Label { get; set; }
        public string? CodigoPostal { get; set; } = string.Empty;
        public bool EsPredeterminada { get; set; }
    }
}