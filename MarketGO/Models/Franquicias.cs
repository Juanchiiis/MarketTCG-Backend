using System.ComponentModel.DataAnnotations.Schema;
namespace MarketGO.Models
{
    [Table("Franquicias")]
    public class Franquicias
    {
        public int Id { get; set; }
        public string Nombre {  get; set; } = string.Empty;
    }
}
