using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using MarketGO.Models;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColeccionesController : ControllerBase
    {
        private readonly IMongoCollection<Colecciones> _coleccionesCollection;

        public ColeccionesController(IMongoDatabase database)
        {
            _coleccionesCollection = database.GetCollection<Colecciones>("colecciones");
        }

        //Endpoint para obtener todas las colecciones
        [HttpGet]
        public async Task<IActionResult> GetColecciones()
        {
            var colecciones = await _coleccionesCollection.Find(Builders<Colecciones>.Filter.Empty).ToListAsync();
            var listaColecciones = colecciones.Select(c => new ColeccionesDTO
            {
                Id = c.Id,
                NombreFranquicia = c.NombreFranquicia,
                NombreColeccion = c.NombreColeccion,
                AnioLanzamiento = c.AnioLanzamiento
            }).ToList();

            return Ok(listaColecciones);
        }

        //Endpoint Post : La ruta que React llamara para crear un nuevo producto
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CrearColeccion(ColeccionCreacionDTO nuevaColeccion)
        {
            try
            {
                var nuevaColeccionBD = new Colecciones
                {
                    FranquiciaId = nuevaColeccion.FranquiciaId,
                    NombreColeccion = nuevaColeccion.NombreColeccion,
                    AnioLanzamiento = nuevaColeccion.FechaLanzamiento.Year
                };

                await _coleccionesCollection.InsertOneAsync(nuevaColeccionBD);

                return Ok(new { message = "Colección creada con exito" });
            }
            catch (Exception ex)
            {
                var errorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                return BadRequest(new
                {
                    message = "Error al crear el producto",
                    detalle = errorReal
                });
            }
        }

        //Endpoint PUT : La ruta que REACT llamara para editar una colección
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarColeccion(string id, ColeccionCreacionDTO coleccionEdtiada)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "ID de la colección no valido" });
            }
            try
            {
                var filter = Builders<Colecciones>.Filter.Eq(c => c.Id, id);
                var ColeccionExistente = await _coleccionesCollection.Find(filter).FirstOrDefaultAsync();

                if (ColeccionExistente == null)
                {
                    return NotFound(new { message = "Coleccion no encontrada" });
                }

                var update = Builders<Colecciones>.Update
                    .Set(c => c.FranquiciaId, coleccionEdtiada.FranquiciaId)
                    .Set(c => c.NombreColeccion, coleccionEdtiada.NombreColeccion)
                    .Set(c => c.AnioLanzamiento, coleccionEdtiada.FechaLanzamiento.Year);

                await _coleccionesCollection.UpdateOneAsync(filter, update);

                return Ok(new { message = "Colección editada con exito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al editar las colecciones", detalle = ex.Message });
            }

        }

        //Endpoint DELETE : La ruta que usara react para eliminar la colección 
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarColeccion(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "La ID de colección no es valida" });

            }

            try
            {
                var filter = Builders<Colecciones>.Filter.Eq(c => c.Id, id);
                var coleccionEncontrada = await _coleccionesCollection.Find(filter).FirstOrDefaultAsync();

                if (coleccionEncontrada == null)
                {
                    return NotFound(new { message = "La coleccion no ha sido encontrada" });
                }

                await _coleccionesCollection.DeleteOneAsync(filter);
                return Ok(new { message = "La coleccion ha sido borrada con exito" });
            }
            catch (Exception ex)
            {
                return BadRequest( new
                {
                    message = "Error al borrar la coleccion",
                    detalle = ex.Message
                });
            }
        }
    }
}
