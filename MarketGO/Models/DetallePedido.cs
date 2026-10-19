using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketGO.Models
{
    [Table("Detalles_Pedido")] // Importante para que coincida con tu tabla SQL
    public class DetallePedido
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string PedidoId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string PublicacionId { get; set; } = string.Empty;
       
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [ForeignKey("PublicacionId")]
        public virtual PublicacionInventario Publicacion { get; set; } 
    }
}