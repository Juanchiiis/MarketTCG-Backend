using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MarketGO.Models;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColeccionesController : ControllerBase
    {
        private readonly MarketGOContext _context;

        // Constructor que recibe el contexto de la base de datos a través de inyección de dependencias
        public ColeccionesController(MarketGOContext context)
        {
            _context = context;
        }

        //Endpoint para obtener todas las colecciones
        [HttpGet]
        public async Task<IActionResult> GetColecciones()
        {

            //Vamos a la base de datos, obtenemos todas las colecciones y las convertimos a una lista
            var listaColecciones = await _context.Colecciones.Join(
                _context.Franquicias, //La tabla secundaria a unir
                colecciones => colecciones.FranquiciaId, //La llave foranea en Colecciones
                franquicias => franquicias.Id, //La llave primeria en Franquicia

                //Creamos un nuevo objeto DTO con la informacion combinada de las dos tablas 
                (colecciones, franquicias) => new ColeccionesDTO
                {
                    Id = colecciones.Id,
                    NombreFranquicia = franquicias.Nombre,
                    NombreColeccion = colecciones.NombreColeccion,
                    AnioLanzamiento = colecciones.AnioLanzamiento
                }
                ).ToListAsync();


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
                //Agregar la nueva coleccion a la base de datos
                _context.Colecciones.Add(nuevaColeccionBD);

                //Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                //Devolver una respuesta indicando que la coleccion se guardo correctamente 
                return Ok(new { message = "Colección creada con exito" });
            }
            catch (Exception ex)
            {
                // Esto extraerá el error exacto de SQL Server (ej: "Conflicto de Foreign Key" o "Texto muy largo")
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
        public async Task<IActionResult> EditarColeccion(int id, ColeccionCreacionDTO coleccionEdtiada)
        {
            //Por seguridad, validamos que la colección es mayor a 0
            if (id <= 0)
            {
                return BadRequest(new { message = "ID de la colección no valido" });
            }
            try
            {
                //Buscar la colleccion si existe en la base de datos
                var ColeccionExistente = await _context.Colecciones.FindAsync(id);
                if (ColeccionExistente == null)
                {
                    return NotFound(new { message = "Coleccion no encontrada" });
                }

                //Actualizar los campos de la coleccion existente con los nuevos valores para la coleccion
                ColeccionExistente.FranquiciaId = coleccionEdtiada.FranquiciaId;
                ColeccionExistente.NombreColeccion = coleccionEdtiada.NombreColeccion;
                ColeccionExistente.AnioLanzamiento = coleccionEdtiada.FechaLanzamiento.Year;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Coleción editada con exito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al editar las colecciones", detalle = ex.Message });
            }

        }

        //Endpoint DELETE : La ruta que usara react para eliminar la colección 
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarColeccion(int id)
        {
            //Por seguridad validamos que el ID del producto sea mayor a 0
            if (id <= 0)
            {
                return BadRequest(new { message = "La ID de colección no es valida" });

            }

            try
            {

                //Buscar la coleccion si exite en la base de datos
                var coleccionEncontrada = await _context.Colecciones.FindAsync(id);
                if (coleccionEncontrada == null)
                {
                    return NotFound(new { message = "La coleccion no ha sido encontrada" });
                }
                _context.Colecciones.Remove(coleccionEncontrada);
                await _context.SaveChangesAsync();
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
    

