using System.ComponentModel.DataAnnotations;

namespace MarketGO.Models
{
    // Representa un producto individual dentro del carrito
    public class ItemCarritoDTO
    {
        [Required]
        public int PublicacionId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }
    }

    // Representa la orden completa de compra
    public class CheckoutDTO
    {
        [Required(ErrorMessage = "Debe seleccionar una dirección de envío.")]
        public int DireccionId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "El carrito no puede estar vacío.")]
        public List<ItemCarritoDTO> Articulos { get; set; } = new List<ItemCarritoDTO>();
    }
}