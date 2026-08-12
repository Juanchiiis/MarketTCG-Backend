namespace MarketGO.Models
{
    public class ColeccionesDTO
    {
        public int Id { get; set; }
        public string NombreFranquicia { get; set; } = string.Empty;
        public string NombreColeccion {  get; set; } = string.Empty;
        public int AnioLanzamiento { get; set; }
    }
}
