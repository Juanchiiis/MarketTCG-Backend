using System.ComponentModel.DataAnnotations;
namespace MarketGO.Models
{
    public class FranquiciasCreacionDTO
    {
        [Required (ErrorMessage = "El campo de franquicia es obligatorio.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La franquicia debe tener un minimo de 6 caracteres y un maximo de 30.")]
        public string Nombre { get; set; } = string.Empty;
    }
}
