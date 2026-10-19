using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MarketGO.Models
{
    public class UsuarioEdicionDTO
    {
        public string Id { get; set; } = string.Empty;

        [Required (ErrorMessage = "El campo de nombre de usuario no puede estar vacio.")]
        [StringLength (50, MinimumLength = 5, ErrorMessage = "El campo de nombre de usuario debe tener un minimo de 5 y un maximo de 50 caracteres.")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string Correo { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Favoritos { get; set; } = new List<string>();

        // La contraseña no existe aquí, asi que el frontend no lo puede enviar 
    }
}
