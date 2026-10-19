using System.ComponentModel.DataAnnotations;

namespace MarketGO.Models
{
    public class DireccionDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La región es obligatoria.")]
        public string Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "La comuna es obligatoria.")]
        public string Comuna { get; set; } = string.Empty;

        [Required(ErrorMessage = "La calle es obligatoria.")]
        public string Calle { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número es obligatorio.")]
        public string Numero { get; set; } = string.Empty;

        public string CodigoPostal { get; set; } = string.Empty;

        public string Label { get; set; }

        public string? Departamento { get; set; }

        public bool EsPredeterminada { get; set; }
    }
}
