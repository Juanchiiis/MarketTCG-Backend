using System.ComponentModel.DataAnnotations.Schema;


namespace MarketGO.Models
{
    [Table("Catalogo_Productos")]
    public class CatalogoProducto
    {
        public int Id { get; set; }
        public int ColeccionId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string TipoProducto { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
        public int UsuarioId { get; set; } // Agregamos la propiedad UsuarioId para la relación con Usuarios

        //Esto permite acceder a los datos del vendedor facilmente en las consultas LINQ
        public virtual Usuarios Usuario { get; set; } 
    }
}
