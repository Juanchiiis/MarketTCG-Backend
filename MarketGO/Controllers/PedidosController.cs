using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly IMongoCollection<Direccion> _direccionesCollection;
        private readonly IMongoCollection<PublicacionInventario> _publicacionesCollection;
        private readonly IMongoCollection<Pedido> _pedidosCollection;
        private readonly IMongoCollection<DetallePedido> _detallesCollection;

        public PedidosController(IMongoDatabase database)
        {
            _direccionesCollection = database.GetCollection<Direccion>("direcciones");
            _publicacionesCollection = database.GetCollection<PublicacionInventario>("publicaciones_inventarios");
            _pedidosCollection = database.GetCollection<Pedido>("pedidos");
            _detallesCollection = database.GetCollection<DetallePedido>("detalles_pedidos");
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutDTO carrito)
        {
            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdToken)) return Unauthorized(new { message = "Token inválido." });

            var filterDir = Builders<Direccion>.Filter.And(
                Builders<Direccion>.Filter.Eq(d => d.Id, carrito.DireccionId),
                Builders<Direccion>.Filter.Eq(d => d.UsuarioId, userIdToken)
            );
            var direccionExiste = await _direccionesCollection.Find(filterDir).AnyAsync();

            if (!direccionExiste) return BadRequest(new { message = "La dirección seleccionada no es válida." });

            try
            {
                var nuevoPedido = new Pedido
                {
                    UsuarioId = userIdToken,
                    DireccionId = carrito.DireccionId,
                    FechaPedido = DateTime.UtcNow,
                    Estado = "Pagado",
                    TotalPagar = 0
                };

                await _pedidosCollection.InsertOneAsync(nuevoPedido);
                decimal totalCalculado = 0;

                foreach (var item in carrito.Articulos)
                {
                    var publicacion = await _publicacionesCollection.Find(p => p.Id == item.PublicacionId).FirstOrDefaultAsync();

                    if (publicacion == null) return BadRequest(new { message = "Un producto del carrito ya no existe." });
                    if (publicacion.Stock < item.Cantidad) return BadRequest(new { message = $"No hay stock suficiente para la publicación ID {item.PublicacionId}." });

                    var subtotal = publicacion.Precio * item.Cantidad;
                    totalCalculado += subtotal;

                    var detalle = new DetallePedido
                    {
                        PedidoId = nuevoPedido.Id,
                        PublicacionId = publicacion.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = publicacion.Precio
                    };
                    await _detallesCollection.InsertOneAsync(detalle);

                    var updateStock = Builders<PublicacionInventario>.Update.Set(p => p.Stock, publicacion.Stock - item.Cantidad);
                    await _publicacionesCollection.UpdateOneAsync(p => p.Id == publicacion.Id, updateStock);
                }

                var updatePedido = Builders<Pedido>.Update.Set(p => p.TotalPagar, totalCalculado);
                await _pedidosCollection.UpdateOneAsync(p => p.Id == nuevoPedido.Id, updatePedido);

                return Ok(new { message = "¡Pedido realizado con éxito!", pedidoId = nuevoPedido.Id, totalPagar = totalCalculado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al procesar la compra", error = ex.Message });
            }
        }

        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> GetMisPedidos()
        {
            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdToken)) return Unauthorized();

            var misPedidos = await _pedidosCollection
                .Find(p => p.UsuarioId == userIdToken)
                .SortByDescending(p => p.FechaPedido)
                .ToListAsync();

            return Ok(misPedidos);
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstado(string id, [FromBody] ActualizarEstadoDTO nuevoEstado)
        {
            var estadosPermitidos = new[] { "Pendiente", "Pagado", "Enviado", "Entregado" };
            if (!estadosPermitidos.Contains(nuevoEstado.Estado)) return BadRequest(new { message = "Estado no válido." });

            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdToken)) return Unauthorized();

            bool esAdmin = User.IsInRole("Admin");

            try
            {
                var pedido = await _pedidosCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (pedido == null) return NotFound(new { message = "Pedido no encontrado." });

                if (!esAdmin)
                {
                    var detalles = await _detallesCollection.Find(d => d.PedidoId == id).ToListAsync();
                    var publicacionIds = detalles.Select(d => d.PublicacionId).ToList();
                    var publicaciones = await _publicacionesCollection.Find(Builders<PublicacionInventario>.Filter.In(p => p.Id, publicacionIds)).ToListAsync();

                    if (!publicaciones.Any(p => p.VendedorId == userIdToken))
                    {
                        return StatusCode(403, new { message = "No tienes permiso para actualizar este pedido." });
                    }
                }

                var update = Builders<Pedido>.Update.Set(p => p.Estado, nuevoEstado.Estado);
                await _pedidosCollection.UpdateOneAsync(p => p.Id == id, update);

                return Ok(new { message = $"El estado del pedido ha sido actualizado a '{nuevoEstado.Estado}'." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el estado.", detalle = ex.Message });
            }
        }
    }
}