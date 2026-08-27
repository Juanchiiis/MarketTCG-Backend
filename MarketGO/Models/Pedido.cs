using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketGO.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int DireccionId { get; set; }
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public virtual ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}