using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protegido por Token
    public class PedidosController : ControllerBase
    {
        private readonly MarketGOContext _context;

        public PedidosController(MarketGOContext context)
        {
            _context = context;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutDTO carrito)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userIdToken))
            {
                return Unauthorized(new { message = "Token inválido." });
            }

            // 1. Validar que la dirección exista y sea realmente de este usuario
            var direccionExiste = await _context.Direcciones
                .AnyAsync(d => d.Id == carrito.DireccionId && d.UsuarioId == userIdToken);

            if (!direccionExiste)
            {
                return BadRequest(new { message = "La dirección seleccionada no es válida." });
            }

            // 2. Iniciamos una transacción segura
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Paso A: Crear la cabecera del pedido
                var nuevoPedido = new Pedido
                {
                    UsuarioId = userIdToken,
                    DireccionId = carrito.DireccionId,
                    FechaPedido = DateTime.Now,
                    Estado = "Pagado",
                    Total = 0 // Lo calcularemos dinámicamente ahora
                };

                _context.Pedidos.Add(nuevoPedido);
                await _context.SaveChangesAsync(); // Guardamos para que SQL genere el ID del Pedido

                decimal totalCalculado = 0;

                // Paso B: Procesar cada artículo del carrito
                foreach (var item in carrito.Articulos)
                {
                    // Buscamos el producto en el inventario
                    var publicacion = await _context.Publicaciones_Inventarios.FindAsync(item.PublicacionId);

                    if (publicacion == null)
                        throw new Exception($"El producto no existe.");

                    if (publicacion.Stock < item.Cantidad)
                        throw new Exception($"No hay stock suficiente para un producto en tu carrito.");

                    // Descontamos el stock
                    publicacion.Stock -= item.Cantidad;

                    // Calculamos el subtotal con el PRECIO REAL de la BD
                    var subtotal = publicacion.Precio * item.Cantidad;
                    totalCalculado += subtotal;

                    // Creamos el detalle histórico
                    var detalle = new DetallePedido
                    {
                        PedidoId = nuevoPedido.Id,
                        PublicacionId = publicacion.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = publicacion.Precio
                    };

                    _context.Detalles_Pedido.Add(detalle);
                }

                // Paso C: Actualizamos el total del pedido con la suma exacta
                nuevoPedido.Total = totalCalculado;
                await _context.SaveChangesAsync();

                // Paso D: Si todo salió perfecto, confirmamos los cambios en SQL
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "¡Pedido realizado con éxito!",
                    pedidoId = nuevoPedido.Id,
                    totalPagar = totalCalculado
                });
            }
            catch (Exception ex)
            {
                // Si algo falla, se cancela todo automáticamente
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Error al procesar la compra", error = ex.Message });
            }
        }
        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> GetMisPedidos()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userIdToken))
            {
                return Unauthorized(new { message = "Token inválido." });
            }

            // Buscamos los pedidos del usuario, ordenados desde el más reciente al más antiguo
            var misPedidos = await _context.Pedidos
                .Include(p => p.Detalles)
                .Where(p => p.UsuarioId == userIdToken)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            return Ok(misPedidos);
        }

        // Endpoint PUT: Actualizar el estado del pedido
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoDTO nuevoEstado)
        {
            // 1. Validar que el estado sea uno de los permitidos
            var estadosPermitidos = new[] { "Pendiente", "Pagado", "Enviado", "Entregado" };
            if (!estadosPermitidos.Contains(nuevoEstado.Estado))
            {
                return BadRequest(new { message = "Estado no válido. Use: Pendiente, Pagado, Enviado o Entregado." });
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userIdToken))
                return Unauthorized(new { message = "Token inválido." });

            bool esAdmin = User.IsInRole("Admin");

            try
            {
                // 2. Buscar el pedido por su ID
                var pedido = await _context.Pedidos
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (pedido == null)
                    return NotFound(new { message = "Pedido no encontrado." });

                if (pedido == null)
                    return NotFound(new { message = "Pedido no encontrado." });

                // 3. Control de Propiedad: Si no es Admin, verificamos si es el dueño de algún producto en este pedido
                if (!esAdmin)
                {
                    // Buscamos manualmente en la tabla de inventario si este usuario es el vendedor de alguno de los items
                    var esVendedorDelPedido = await _context.Detalles_Pedido
                        .Where(d => d.PedidoId == id)
                        .Join(_context.Publicaciones_Inventarios,
                              detalle => detalle.PublicacionId,
                              pub => pub.Id,
                              (detalle, pub) => pub.VendedorId)
                        .AnyAsync(vendedorId => vendedorId == userIdToken);

                    if (!esVendedorDelPedido)
                    {
                        return StatusCode(403, new { message = "No tienes permiso para actualizar un pedido que no contiene tus productos." });
                    }
                }

                // 4. Actualizamos y guardamos
                pedido.Estado = nuevoEstado.Estado;
                await _context.SaveChangesAsync();

                return Ok(new { message = $"El estado del pedido ha sido actualizado a '{nuevoEstado.Estado}'." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el estado.", detalle = ex.Message });
            }
        }
    }
}