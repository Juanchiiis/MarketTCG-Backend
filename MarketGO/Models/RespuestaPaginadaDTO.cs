namespace MarketGO.Models
{
    // Usamos genéricos <T> para que esta paginación te sirva para cualquier cosa en el futuro
    public class RespuestaPaginadaDTO<T>
    {
        public int TotalItems { get; set; }
        public int TotalPaginas { get; set; }
        public int PaginaActual { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }
}