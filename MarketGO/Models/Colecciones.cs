using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketGO.Models
{
    [Table("Colecciones")]
    public class Colecciones
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string FranquiciaId { get; set; } = string.Empty;
        public string NombreColeccion { get; set; } = string.Empty;
        public string NombreFranquicia { get; set; } = string.Empty;
        public int AnioLanzamiento { get; set; }
    }
}
