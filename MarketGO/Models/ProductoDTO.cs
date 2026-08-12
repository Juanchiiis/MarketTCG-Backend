namespace MarketGO.Models
{
    public class ProductoDTO
    {
        public int Id { get; set; }

        //En lugar del numero ID, enviamos el texto real.
        public string NombreColeccion { get; set; } = string.Empty;
        public string NombreProducto {  get; set; } = string.Empty;
        public string SKU {  get; set; } = string.Empty;
        public string TipoProducto {  get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;

        public int VendedorId { get; set; } // Agregamos la propiedad VendedorId para la relación con Usuarios
        public string NombreVendedor { get; set; } = string.Empty; // Agregamos la propiedad NombreVendedor para mostrar el nombre del vendedor
    }
}
