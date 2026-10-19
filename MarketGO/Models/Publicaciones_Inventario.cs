using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketGO.Models
{
    [Table("Publicaciones_Inventario")]
    public class PublicacionInventario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductoCatalogoId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string VendedorId { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Notas { get; set; } = string.Empty;

        [ForeignKey("ProductoCatalogoId")]
        public virtual CatalogoProducto ProductoCatalogo { get; set; }
    }
}
