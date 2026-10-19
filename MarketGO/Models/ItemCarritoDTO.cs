using System.ComponentModel.DataAnnotations;

namespace MarketGO.Models
{
    // Representa un producto individual dentro del carrito
    public class ItemCarritoDTO
    {
        [Required]
        public string PublicacionId { get; set; } = string.Empty;

        [Required]
        [Range(1, 100, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }
    }

    // Representa la orden completa de compra
    public class CheckoutDTO
    {
        [Required(ErrorMessage = "Debe seleccionar una dirección de envío.")]
        public string DireccionId { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "El carrito no puede estar vacío.")]
        public List<ItemCarritoDTO> Articulos { get; set; } = new List<ItemCarritoDTO>();
    }
}