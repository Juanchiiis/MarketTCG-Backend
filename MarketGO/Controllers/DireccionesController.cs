using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DireccionesController : ControllerBase
    {
        private readonly MarketGOContext _context;

        public DireccionesController(MarketGOContext context)
        {
            _context = context;
        }

        // GET: /api/Direcciones/mis-direcciones
        [HttpGet("mis-direcciones")]
        public async Task<IActionResult> GetMisDirecciones()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userIdToken)) return Unauthorized();

            var misDirecciones = await _context.Direcciones
                .Where(d => d.UsuarioId == userIdToken)
                .ToListAsync();

            return Ok(misDirecciones);
        }

        // POST: /api/Direcciones
        [HttpPost]
        public async Task<IActionResult> CrearDireccion(DireccionDTO nuevaDireccion)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userIdToken)) return Unauthorized();

            try
            {
                bool tieneDirecciones = await _context.Direcciones.AnyAsync(d => d.UsuarioId == userIdToken);

                // Si se marca como predeterminada, quitamos esa marca a las demás
                if (nuevaDireccion.EsPredeterminada)
                {
                    var direccionesExistentes = await _context.Direcciones.Where(d => d.UsuarioId == userIdToken).ToListAsync();
                    foreach (var d in direccionesExistentes) d.EsPredeterminada = false;
                }

                var direccionBD = new Direccion
                {
                    UsuarioId = userIdToken,
                    Label = nuevaDireccion.Label ?? "Casa", // <-- Usamos el Label (Etiqueta) que viene del frontend
                    Region = nuevaDireccion.Region,
                    Comuna = nuevaDireccion.Comuna,
                    Calle = nuevaDireccion.Calle,
                    Numero = nuevaDireccion.Numero ?? "",
                    Departamento = nuevaDireccion.Departamento ?? "",
                    CodigoPostal = nuevaDireccion.CodigoPostal ?? "",
                    EsPredeterminada = nuevaDireccion.EsPredeterminada || !tieneDirecciones
                };

                _context.Direcciones.Add(direccionBD);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Dirección guardada", id = direccionBD.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al guardar", error = ex.Message });
            }
        }

        // PUT: /api/Direcciones/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarDireccion(int id, DireccionDTO direccionEditada)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userIdToken)) return Unauthorized();

            var direccionBD = await _context.Direcciones.FindAsync(id);

            if (direccionBD == null) return NotFound(new { message = "Dirección no encontrada." });
            if (direccionBD.UsuarioId != userIdToken) return StatusCode(403, new { message = "No tienes permiso para editar esta dirección." });

            try
            {
                if (direccionEditada.EsPredeterminada)
                {
                    var otrasDirecciones = await _context.Direcciones.Where(d => d.UsuarioId == userIdToken && d.Id != id).ToListAsync();
                    foreach (var d in otrasDirecciones) d.EsPredeterminada = false;
                }

                direccionBD.Label = direccionEditada.Label;
                direccionBD.Region = direccionEditada.Region;
                direccionBD.Comuna = direccionEditada.Comuna;
                direccionBD.Calle = direccionEditada.Calle;
                direccionBD.Numero = direccionEditada.Numero ?? "";
                direccionBD.Departamento = direccionEditada.Departamento ?? "";
                direccionBD.CodigoPostal = direccionEditada.CodigoPostal ?? "";
                direccionBD.EsPredeterminada = direccionEditada.EsPredeterminada;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Dirección actualizada con éxito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar", error = ex.Message });
            }
        }

        // DELETE: /api/Direcciones/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarDireccion(int id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userIdToken)) return Unauthorized();

            var direccionBD = await _context.Direcciones.FindAsync(id);

            if (direccionBD == null) return NotFound();
            if (direccionBD.UsuarioId != userIdToken) return StatusCode(403);

            try
            {
                _context.Direcciones.Remove(direccionBD);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Dirección eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al eliminar", error = ex.Message });
            }
        }
    }
}