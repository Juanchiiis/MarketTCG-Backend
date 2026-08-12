namespace MarketGO.Models
{
    public class PublicacionInventarioDTO
    {
        public int Id { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string NombreVendedor { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Notas {  get; set; } = string.Empty;
    }
}
