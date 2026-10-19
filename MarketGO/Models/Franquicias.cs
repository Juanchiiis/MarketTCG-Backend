using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace MarketGO.Models
{
    [Table("Franquicias")]
    public class Franquicias
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string Nombre {  get; set; } = string.Empty;
    }
}
