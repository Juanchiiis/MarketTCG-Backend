using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace MarketGO.Models
{
    [Table("Catalogo_Productos")]
    public class CatalogoProducto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string ColeccionId { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string TipoProducto { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty; // Agregamos la propiedad UsuarioId para la relación con Usuarios

        //Esto permite acceder a los datos del vendedor facilmente en las consultas LINQ
        public virtual Usuarios Usuario { get; set; } 
    }
}
