using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketGO.Models
{
    public class Direccion
    {
        [Key]
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Region { get; set; } = string.Empty;
        public string Comuna { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Departamento { get; set; }
        public bool EsPredeterminada { get; set; }
    }
}