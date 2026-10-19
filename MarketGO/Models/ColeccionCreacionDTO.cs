using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
namespace MarketGO.Models
{
    public class ColeccionCreacionDTO
    {
        [Required(ErrorMessage = " Debes seleccionar a que franquicia pertenece la colección.")]
        public string FranquiciaId { get; set; } = string.Empty;

        [Required (ErrorMessage = "El campo de Nombre de la colección no puede estar vacio")]
        [StringLength (150, MinimumLength = 8, ErrorMessage = "El campo de nombre de coleccion debe tener un minimo de 8 y un maximo de 150 caracteres.")]
        public string NombreColeccion { get; set; } = string.Empty;

        [Required (ErrorMessage = "La fecha de lanzamiento no puede quedar con el campo vacio.")]
        [DataType (DataType.Date, ErrorMessage = "El formato de fecha no es valido.")]
        [Range (typeof(DateTime), "01/01/1900", "31/12/2100", ErrorMessage = "La fecha de lanzamiento no es valida.")]
        public DateTime FechaLanzamiento { get; set; }

    }
}
