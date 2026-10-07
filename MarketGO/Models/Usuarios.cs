using System.ComponentModel.DataAnnotations.Schema;

namespace MarketGO.Models
{
    [Table("Usuarios")]
    public class Usuarios
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo {  get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public string Rol { get; set; } = "Cliente";


    }
}
 