using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketGO.Models
{
    [Table("Detalles_Pedido")] // Importante para que coincida con tu tabla SQL
    public class DetallePedido
    {
        [Key]
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int PublicacionId { get; set; }
       
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [ForeignKey("PublicacionId")]
        public virtual PublicacionInventario Publicacion { get; set; } 
    }
}