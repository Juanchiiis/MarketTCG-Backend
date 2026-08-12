using System.ComponentModel.DataAnnotations;

namespace MarketGO.Models
{
    public class UsuarioEdicionDTO
    {
        public int Id { get; set; }

        [Required (ErrorMessage = "El campo de nombre de usuario no puede estar vacio.")]
        [StringLength (50, MinimumLength = 5, ErrorMessage = "El campo de nombre de usuario debe tener un minimo de 5 y un maximo de 50 caracteres.")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string Correo { get; set; } = string.Empty;

        // La contraseña no existe aquí, asi que el frontend no lo puede enviar 
    }
}
