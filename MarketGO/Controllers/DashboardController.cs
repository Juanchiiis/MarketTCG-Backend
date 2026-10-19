using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IMongoCollection<Usuarios> _usuariosCollection;
        private readonly IMongoCollection<Pedido> _pedidosCollection;
        private readonly IMongoCollection<PublicacionInventario> _publicacionesCollection;
        private readonly IMongoCollection<CatalogoProducto> _productosCollection;

        public DashboardController(IMongoDatabase database)
        {
            _usuariosCollection = database.GetCollection<Usuarios>("usuarios");
            _pedidosCollection = database.GetCollection<Pedido>("pedidos");
            _publicacionesCollection = database.GetCollection<PublicacionInventario>("publicaciones_inventarios");
            _productosCollection = database.GetCollection<CatalogoProducto>("catalogo_productos");
        }

        // Endpoint GET: Métricas Generales (Ventas, Usuarios, Pedidos)
        [HttpGet("metricas")]
        public async Task<IActionResult> GetMetricasPrincipales()
        {
            try
            {
                int totalUsuarios = (int)await _usuariosCollection.CountDocumentsAsync(Builders<Usuarios>.Filter.Empty);

                var filterPedidos = Builders<Pedido>.Filter.Ne(p => p.Estado, "Pendiente");
                int totalPedidos = (int)await _pedidosCollection.CountDocumentsAsync(filterPedidos);

                var pedidosCompletados = await _pedidosCollection
                    .Find(filterPedidos)
                    .Project(p => p.TotalPagar)
                    .ToListAsync();

                decimal ingresosTotales = pedidosCompletados.Sum();

                return Ok(new
                {
                    UsuariosRegistrados = totalUsuarios,
                    PedidosCompletados = totalPedidos,
                    IngresosTotales = ingresosTotales
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al calcular métricas", detalle = ex.Message });
            }
        }

        // Endpoint GET: Alerta de productos sin stock (Stock Cero)
        [HttpGet("alertas-stock")]
        public async Task<IActionResult> GetAlertasStock()
        {
            try
            {
                var filterStock = Builders<PublicacionInventario>.Filter.Eq(p => p.Stock, 0);
                var publicacionesSinStock = await _publicacionesCollection.Find(filterStock).ToListAsync();
                var productos = await _productosCollection.Find(Builders<CatalogoProducto>.Filter.Empty).ToListAsync();

                var productosSinStock = publicacionesSinStock.Select(pub =>
                {
                    var producto = productos.FirstOrDefault(p => p.Id == pub.ProductoCatalogoId);
                    return new
                    {
                        PublicacionId = pub.Id,
                        NombreProducto = producto?.NombreProducto ?? "Desconocido",
                        SKU = producto?.SKU ?? "N/A",
                        VendedorId = pub.VendedorId
                    };
                }).ToList();

                return Ok(new
                {
                    TotalAlertas = productosSinStock.Count,
                    Productos = productosSinStock
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener alertas de stock", detalle = ex.Message });
            }
        }
    }
}