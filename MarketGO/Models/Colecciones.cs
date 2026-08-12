using System.ComponentModel.DataAnnotations.Schema;

namespace MarketGO.Models
{
    [Table("Colecciones")]
    public class Colecciones
    {
        public int Id { get; set; }
        public int FranquiciaId { get; set; }
        public string NombreColeccion { get; set; } = string.Empty;
        public int AnioLanzamiento { get; set; } 
    }
}
