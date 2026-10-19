using MarketGO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FranquiciasController : ControllerBase
    {
        private readonly IMongoCollection<Franquicias> _franquiciasCollection;

        public FranquiciasController(IMongoDatabase database)
        {
            _franquiciasCollection = database.GetCollection<Franquicias>("franquicias");
        }

        //Endpoint GET : La ruta que REACT llamara para obtener la lista de franquicias
        [HttpGet]
        public async Task<IActionResult> GetFranquicias()
        {
            var listaFranquicias = await _franquiciasCollection.Find(Builders<Franquicias>.Filter.Empty).ToListAsync();
            return Ok(listaFranquicias);
        }

        //Endpoint POST: La ruta que react llamara para crear un nueva Franquicia
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CrearFranquicia(FranquiciasCreacionDTO nuevaFranquicia)
        {
            try
            {
                var nuevaFranquiciaBD = new Franquicias
                {
                    Nombre = nuevaFranquicia.Nombre
                };

                await _franquiciasCollection.InsertOneAsync(nuevaFranquiciaBD);

                return Ok(new { message = "La Franquicia fue creada con exito" });
            }
            catch(Exception ex)
            {
                return BadRequest(new {message = "Error al crear el producto", detalle = ex.Message});
            }
        }

        //Endpoint PUT : LA ruta que REACT llamara para editar la franquicia
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarFranquicia(string id, FranquiciasCreacionDTO editarFranquicia)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "ID de la franquicia no es valida" });
            }

            try
            {
                var filter = Builders<Franquicias>.Filter.Eq(f => f.Id, id);
                var ExisteFranquicia = await _franquiciasCollection.Find(filter).FirstOrDefaultAsync();

                if (ExisteFranquicia == null)
                {
                    return NotFound(new { message = "Franquicia no encontrada" });
                }

                var update = Builders<Franquicias>.Update.Set(f => f.Nombre, editarFranquicia.Nombre);
                await _franquiciasCollection.UpdateOneAsync(filter, update);

                return Ok(new { message = "Franquicia editada con exito " });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al editar la franquicia", detalle = ex.Message });
            }
        }

        //Endpoint DELETE : La ruta que usara react para eliminar la franquicia
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarFranquicia(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "La ID de franquicia no es valida"});
            }

            try
            {
                var filter = Builders<Franquicias>.Filter.Eq(f => f.Id, id);
                var existeFranquicia = await _franquiciasCollection.Find(filter).FirstOrDefaultAsync();

                if (existeFranquicia == null)
                {
                    return NotFound(new { message = "La coleccion no ha sido encontrada" });
                }

                await _franquiciasCollection.DeleteOneAsync(filter);
                return Ok(new { message = "La franquicia ha sido borrada con exito" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al eliminar la franquicia", detalle = ex.Message });
            }
        }
    }
}
