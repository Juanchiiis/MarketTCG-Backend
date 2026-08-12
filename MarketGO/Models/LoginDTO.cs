using System.ComponentModel.DataAnnotations;

namespace MarketGO.Models
{
    public class LoginDTO
    {
        [Required (ErrorMessage = "El campo de Correo no puede quedar vacio.")]
        [EmailAddress (ErrorMessage ="Este no es un correo valido.")]
        public string Correo { get; set; } = string.Empty;
        [Required (ErrorMessage = "El campo de contraseña no puede estar vacio.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El campo de contraseña debe contar con un minimo de 5 y un maximo de 100 caracteres." )]
        public string Password { get; set; } = string.Empty;
    } 
    
}
