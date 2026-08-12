using System.ComponentModel.DataAnnotations.Schema;

namespace MarketGO.Models
{
    [Table("Publicaciones_Inventario")]
    public class PublicacionInventario
    {
        public int Id { get; set; }
        public int ProductoCatalogoId { get; set; }
        public int VendedorId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Notas { get; set; } = string.Empty;
    }
}
