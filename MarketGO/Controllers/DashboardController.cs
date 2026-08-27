using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // 🔒 SEGURIDAD ABSOLUTA: Solo Administradores
    public class DashboardController : ControllerBase
    {
        private readonly MarketGOContext _context;

        public DashboardController(MarketGOContext context)
        {
            _context = context;
        }

        // Endpoint GET: Métricas Generales (Ventas, Usuarios, Pedidos)
        [HttpGet("metricas")]
        public async Task<IActionResult> GetMetricasPrincipales()
        {
            try
            {
                // 1. Total de usuarios registrados
                int totalUsuarios = await _context.Usuarios.CountAsync();

                // 2. Total de pedidos exitosos (Excluimos los que estén "Pendientes" de pago si los hubiera)
                int totalPedidos = await _context.Pedidos
                    .Where(p => p.Estado != "Pendiente")
                    .CountAsync();

                // 3. Suma total de dinero movido en la plataforma
                decimal ingresosTotales = await _context.Pedidos
                    .Where(p => p.Estado != "Pendiente")
                    .SumAsync(p => p.Total);

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
                // Buscamos publicaciones con 0 stock y hacemos un JOIN rápido para saber cómo se llama el producto
                var productosSinStock = await (from publicacion in _context.Publicaciones_Inventarios
                                               join producto in _context.Catalogo_Productos
                                               on publicacion.ProductoCatalogoId equals producto.Id
                                               where publicacion.Stock == 0
                                               select new
                                               {
                                                   PublicacionId = publicacion.Id,
                                                   NombreProducto = producto.NombreProducto,
                                                   SKU = producto.SKU,
                                                   VendedorId = publicacion.VendedorId
                                               }).ToListAsync();

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