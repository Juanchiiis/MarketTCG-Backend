using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MarketGO.Models
{
    public class ArticuloPedido
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string PublicacionId { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
    public class Pedido
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string DireccionId { get; set; } = string.Empty;

        public virtual Direccion Direccion { get; set; }
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPagar { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public virtual ICollection<ArticuloPedido> Articulos { get; set; } = new List<ArticuloPedido>();
    }
}