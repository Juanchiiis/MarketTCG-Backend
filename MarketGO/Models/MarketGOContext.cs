using Microsoft.EntityFrameworkCore;

namespace MarketGO.Models
{
    /* Heredamos de DBContext (esto es POO puro: MarketGOContext es un tipo de DBContext,
     * por lo tanto, tiene todas las propiedades y métodos de DBContext) */

    public class MarketGOContext : DbContext
    {
        // Constructor que recibe opciones de configuración para la base de datos
        public MarketGOContext(DbContextOptions<MarketGOContext> options) : base(options)
        {
        }
        // Propiedad que representa la tabla de Productos en la base de datos
        public DbSet<CatalogoProducto> Catalogo_Productos { get; set; }
        public DbSet<Colecciones> Colecciones { get; set; }
        public DbSet<Franquicias> Franquicias { get; set; }
        public DbSet<PublicacionInventario> Publicaciones_Inventarios { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }



    }

}