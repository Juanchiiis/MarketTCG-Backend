using MarketGO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Pkcs;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FranquiciasController : ControllerBase
    {
        private readonly MarketGOContext _context;

        //El constructor recibe el contexto de la base de datos para poder usarlo en los métodos
        public FranquiciasController(MarketGOContext context)
        {
            _context = context;
        }

        //Endpoint GET : La ruta que REACT llamara para obtener la lista de franquicias
        [HttpGet]
        public async Task<IActionResult> GetFranquicias()
        {
            //Vamos a la base de datos, tomamos la tabla de Franquicia y la convertimos en una lista
            var listaFranquicias = await _context.Franquicias.ToListAsync();
            //El metodo ok() devuelve un "Status 200" y automaticamente convierte la lista de franquicias a formato JSON para que REACT pueda entenderlo
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
                //Agregar la nueva franquicia a la base de datos
                _context.Franquicias.Add(nuevaFranquiciaBD);

                //Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                //Devolver una respuesta indicando que la franquicia se guardo correctamente
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
        public async Task<IActionResult> EditarFranquicia(int id, FranquiciasCreacionDTO editarFranquicia)
        {
            //Por seguridad, validamos que la franquica es mayor  a 0
            if (id <= 0)
            {
                return BadRequest(new { message = "ID de la franquicia no es valida" });
            }

            try
            {
                //Buscar la franquicia si existe en la base  de datos
                var ExisteFranquicia = await _context.Franquicias.FindAsync(id);
                if (ExisteFranquicia == null)
                {
                    return NotFound(new { message = "Franquicia no encontrada" });
                }

                //Actualizamos los campos de la franquicia existente
                
                ExisteFranquicia.Nombre = editarFranquicia.Nombre;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Franquicia editada con exito " });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al editar la franquicia", detalle =ex.Message });
            }
        }

        //Endpoint DELETE : La ruta que usara react para eliminar la franquicia
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarFranquicia(int id )
        {
            //Por seguridad validamos si el id de franquicia es mayor a 0
            if(id <= 0)
            {
                return BadRequest(new { message = "La ID de franquicia no es valida"});
            }

            try
            {
                //Buscar la franquicia si existe en la base de datos
                var existeFranquicia = await _context.Franquicias.FindAsync(id);
                if (existeFranquicia == null)
                {
                    return NotFound(new { message = "La coleccion no ha sido encontrada" });
                }

                _context.Franquicias.Remove(existeFranquicia);
                await _context.SaveChangesAsync();
                return Ok(new { message = "La franquicia ha sido borrada con exito" });
            }
            catch (Exception ex)
            {
                return BadRequest( new { message = "Error al eliminar la franquicia", detalle = ex.Message });
            }

        }

    }
}
