using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MarketGO.Models
{
    public class ProductoCreacionDTO
    {
        //Validamos que envíen un ID válido de colección (mayor a 0)
        [Required(ErrorMessage = "Debes seleccionar a qué colección pertenece el producto.")]
        [Range(1, int.MaxValue, ErrorMessage = "La id de colección no es válida.")]
        public int ColeccionId { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El nombre debe tener entre 5 a 100 caracteres.")]
        public string NombreProducto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código SKU es obligatorio.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "El código de SKU debe tener entre 3 a 20 caracteres.")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "El típo de producto es obligatorio.")]
        [StringLength(50, ErrorMessage = "El tipo de producto no puede exceder de los 50 caracteres.")]
        public string TipoProducto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La imagen es obligatoria para poder publicar el producto.")]
        [Url(ErrorMessage = "El enlace de la imagen no es válido. Debe empezar cont https:// o http://.")]
        public string ImagenUrl { get; set; } = string.Empty;


        
    }
}
