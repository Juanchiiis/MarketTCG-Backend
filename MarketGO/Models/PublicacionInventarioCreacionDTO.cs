using System.ComponentModel.DataAnnotations;

namespace MarketGO.Models
{
    public class PublicacionInventarioCreacionDTO
    {
        [Required(ErrorMessage = "El campo de producto no puede quedar vacio ")]
        public string ProductoCatalogoId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo de estado no puede quedar vacio ")]
        [StringLength(50, ErrorMessage = "Estado no puede exceder 50 caracteres")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo de precio no puede quedar vacio ")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Precio debe ser mayor o igual a 0")]
        public decimal Precio { get; set; }

        [Required (ErrorMessage = "El campo de stock no puede quedar vacio ")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock debe ser 0 o mayor")]
        public int Stock { get; set; }

        [StringLength(500, ErrorMessage = "Notas no puede exceder 500 caracteres")]
        public string Notas { get; set; } = string.Empty;
    }
}
