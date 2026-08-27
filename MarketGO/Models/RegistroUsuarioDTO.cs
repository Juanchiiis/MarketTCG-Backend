using System.ComponentModel.DataAnnotations;

namespace MarketGO.Models
{
    public class RegistroUsuarioDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "El nombre de usuario debe tener un minimo de 5 y un maximo de 50 caracteres.")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electronico es obigatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "La contraseña debe tener un minimo de 5 y un maximo de 100 caracteres.")]

        public string Password { get; set; } = string.Empty;
    }
}
