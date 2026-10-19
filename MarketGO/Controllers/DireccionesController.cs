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
    public class DireccionesController : ControllerBase
    {
        private readonly IMongoCollection<Direccion> _direccionesCollection;

        public DireccionesController(IMongoDatabase database)
        {
            _direccionesCollection = database.GetCollection<Direccion>("direcciones");
        }

        [HttpGet("mis-direcciones")]
        public async Task<IActionResult> GetMisDirecciones()
        {
            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdToken)) return Unauthorized();

            var misDirecciones = await _direccionesCollection.Find(d => d.UsuarioId == userIdToken).ToListAsync();
            return Ok(misDirecciones);
        }

        [HttpPost]
        public async Task<IActionResult> CrearDireccion(DireccionDTO nuevaDireccion)
        {
            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdToken)) return Unauthorized();

            try
            {
                bool tieneDirecciones = await _direccionesCollection.Find(d => d.UsuarioId == userIdToken).AnyAsync();

                if (nuevaDireccion.EsPredeterminada && tieneDirecciones)
                {
                    var update = Builders<Direccion>.Update.Set(d => d.EsPredeterminada, false);
                    await _direccionesCollection.UpdateManyAsync(d => d.UsuarioId == userIdToken, update);
                }

                var direccionBD = new Direccion
                {
                    UsuarioId = userIdToken,
                    Label = nuevaDireccion.Label ?? "Casa",
                    Region = nuevaDireccion.Region,
                    Comuna = nuevaDireccion.Comuna,
                    Calle = nuevaDireccion.Calle,
                    Numero = nuevaDireccion.Numero ?? "",
                    Departamento = nuevaDireccion.Departamento ?? "",
                    CodigoPostal = nuevaDireccion.CodigoPostal ?? "",
                    EsPredeterminada = nuevaDireccion.EsPredeterminada || !tieneDirecciones
                };

                await _direccionesCollection.InsertOneAsync(direccionBD);
                return Ok(new { message = "Dirección guardada", id = direccionBD.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al guardar", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarDireccion(string id, DireccionDTO direccionEditada)
        {
            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdToken)) return Unauthorized();

            var direccionBD = await _direccionesCollection.Find(d => d.Id == id).FirstOrDefaultAsync();

            if (direccionBD == null) return NotFound(new { message = "Dirección no encontrada." });
            if (direccionBD.UsuarioId != userIdToken) return StatusCode(403, new { message = "Acceso denegado." });

            try
            {
                if (direccionEditada.EsPredeterminada)
                {
                    var filterOtras = Builders<Direccion>.Filter.And(
                        Builders<Direccion>.Filter.Eq(d => d.UsuarioId, userIdToken),
                        Builders<Direccion>.Filter.Ne(d => d.Id, id)
                    );
                    var updateOtras = Builders<Direccion>.Update.Set(d => d.EsPredeterminada, false);
                    await _direccionesCollection.UpdateManyAsync(filterOtras, updateOtras);
                }

                var update = Builders<Direccion>.Update
                    .Set(d => d.Label, direccionEditada.Label)
                    .Set(d => d.Region, direccionEditada.Region)
                    .Set(d => d.Comuna, direccionEditada.Comuna)
                    .Set(d => d.Calle, direccionEditada.Calle)
                    .Set(d => d.Numero, direccionEditada.Numero ?? "")
                    .Set(d => d.Departamento, direccionEditada.Departamento ?? "")
                    .Set(d => d.CodigoPostal, direccionEditada.CodigoPostal ?? "")
                    .Set(d => d.EsPredeterminada, direccionEditada.EsPredeterminada);

                await _direccionesCollection.UpdateOneAsync(d => d.Id == id, update);
                return Ok(new { message = "Dirección actualizada con éxito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarDireccion(string id)

        {
            if (id.Length != 24) return BadRequest(new { message = "ID inválido." });
            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdToken)) return Unauthorized();

            var direccionBD = await _direccionesCollection.Find(d => d.Id == id).FirstOrDefaultAsync();
            if (direccionBD == null) return NotFound();
            if (direccionBD.UsuarioId != userIdToken) return StatusCode(403);

            await _direccionesCollection.DeleteOneAsync(d => d.Id == id);
            return Ok(new { message = "Dirección eliminada correctamente" });
        }
    }
}