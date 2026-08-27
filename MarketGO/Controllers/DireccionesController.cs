using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Obligamos a que el usuario esté logueado (tenga Token)
    public class DireccionesController : ControllerBase
    {
        private readonly MarketGOContext _context;

        public DireccionesController(MarketGOContext context)
        {
            _context = context;
        }

        // Endpoint GET: React lo llamará para mostrar las direcciones en el Checkout
        [HttpGet("mis-direcciones")]
        public async Task<IActionResult> GetMisDirecciones()
        {
            // Extraemos la ID de forma segura desde el Token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userIdToken))
            {
                return Unauthorized(new { message = "Token inválido." });
            }

            // Buscamos solo las direcciones que le pertenecen a este usuario
            var misDirecciones = await _context.Direcciones
                .Where(d => d.UsuarioId == userIdToken)
                .ToListAsync();

            return Ok(misDirecciones);
        }

        // Endpoint POST: React lo llamará cuando el usuario guarde una nueva dirección
        [HttpPost]
        public async Task<IActionResult> CrearDireccion(DireccionDTO nuevaDireccion)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userIdToken))
            {
                return Unauthorized(new { message = "Token inválido." });
            }

            try
            {
                // Un pequeño truco UX: Si es su primera dirección, la marcamos como predeterminada automáticamente
                bool tieneDirecciones = await _context.Direcciones.AnyAsync(d => d.UsuarioId == userIdToken);

                var direccionBD = new Direccion
                {
                    UsuarioId = userIdToken,
                    Region = nuevaDireccion.Region,
                    Comuna = nuevaDireccion.Comuna,
                    Calle = nuevaDireccion.Calle,
                    Numero = nuevaDireccion.Numero,
                    Departamento = nuevaDireccion.Departamento,
                    EsPredeterminada = !tieneDirecciones
                };

                _context.Direcciones.Add(direccionBD);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Dirección guardada con éxito", id = direccionBD.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Ocurrió un error al guardar la dirección", error = ex.Message });
            }
        }
    }
}