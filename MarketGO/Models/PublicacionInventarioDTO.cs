namespace MarketGO.Models
{
    public class PublicacionInventarioDTO
    {
        public int Id { get; set; }
        public int ProductoCatalogoId { get; set; }
        public int ColeccionId { get; set; }
        public string SKU { get; set; }
        public string TipoProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
        public string NombreVendedor { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Notas { get; set; } = string.Empty;
        public string NombreColeccion { get; set; } = string.Empty;
        public string NombreFranquicia { get; set; } = string.Empty;
    }
}
