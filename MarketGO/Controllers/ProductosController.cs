using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MarketGO.Models;


namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly MarketGOContext _context;

        //El constructor recibe el contexto de la base de datos para poder usarlo en los métodos
        public ProductosController(MarketGOContext context)
        {
            _context = context;
        }

        //Endpoint GET : La ruta que REACT llamara para obtener la lista de productos
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            //Usamos LINQ para hacer un JOIN entre Cataloogo_Productos y Colecciones
            var listaProductos =  await _context.Catalogo_Productos.Join(
                    _context.Colecciones, //La tabla secundaria a unir
                    producto => producto.ColeccionId, //La llave foranea en Producto
                    coleccion => coleccion.Id, //La llave primaria en Coleccion

                    //Creamos un nuevo objeto DTO con la información combinada
                    (producto, coleccion) => new ProductoDTO
                    {

                        Id = producto.Id,
                        NombreColeccion = coleccion.NombreColeccion, //Sacamos el nombre de otra tabla
                        NombreProducto = producto.NombreProducto,
                        SKU = producto.SKU,
                        TipoProducto = producto.TipoProducto,
                        ImagenUrl = producto.ImagenUrl
                    }
                ).ToListAsync();

            //El metodo ok() devuelve un "Status 200" y automaticamente convierte la lista de productos a formato JSON para que REACT pueda entenderlo
            return Ok(listaProductos);
        }

        //Endpoint POST : La ruta que REACT llamara para crear un nuevo producto
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CrearProducto(ProductoCreacionDTO   nuevoProducto)
        {

            try
            {
                // 2. Extraer el ID del usuario del token para asignarlo como dueño del producto
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int userIdToken = 0;

                if(!int.TryParse(userIdString, out userIdToken))
                {
                    return Unauthorized(new { message = "Token invalido" });
                }
                // Creamos una entidad limpia de CatalogoProducto lista para la base de datos

                var nuevoProductoBD = new CatalogoProducto
                {
                    ColeccionId = nuevoProducto.ColeccionId,
                    NombreProducto = nuevoProducto.NombreProducto,
                    SKU = nuevoProducto.SKU,
                    TipoProducto = nuevoProducto.TipoProducto,
                    ImagenUrl = nuevoProducto.ImagenUrl,

                    // Le inyectamos el ID seguro del dueño que le sacamos el token
                    UsuarioId = userIdToken
                };



                // Agregar el nuevo producto a la base de datos
                _context.Catalogo_Productos.Add(nuevoProductoBD);

                // Guardar los cambios en la base de datos
               await _context.SaveChangesAsync();//Si el coleccion ID no existe en la tabla de colecciones, se lanzara una excepcion que sera capturada por el bloque catch

                // Devolver una respuesta indicando que el producto fue creado exitosamente
                return Ok(new { message = "Producto creado exitosamente", id = nuevoProductoBD.Id });
            }
            catch(Exception ex)
            {
                //Salta el error si algo sale mal evitando que se caiga la aplicacion
                var ErrorReal = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest( new {message = "Error al crear el producto", detalle = ErrorReal });
            }
        }

        //Endpoint PUT : La ruta que REACT llamara para editar un producto existente
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarProducto(int id, ProductoCreacionDTO productoEditado)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int userIdToken = 0;

            if (!int.TryParse(userIdString, out  userIdToken))
            {
                return Unauthorized(new { message = "Token invalido " });
            }

            //Por seguridad , validmos que el producto editado tenga un ID valido (mayor a 0)
            if (id  <= 0)
            {
                return BadRequest(new { message = "ID del producto no valido" });
            }
            try
            {


                //Buscar el producto si existe en la base de datos
                var ProductoExistente = await _context.Catalogo_Productos.FindAsync(id);
                if (ProductoExistente == null)
                {
                    return NotFound(new { message = "Producto no encontrado", id = id });
                }

                //Control de propiedad(OWNERSHIP)
                //Comparacion el dueño de la cuenta con el token
                if (ProductoExistente.UsuarioId != userIdToken && !User.IsInRole("Admin"))
                {
                    return StatusCode(403, new { message = "No tienes permiso para editar este producto" });
                }

                //Actualizar los campos del producto existente con los nuevos valores
                ProductoExistente.ColeccionId = productoEditado.ColeccionId;
                ProductoExistente.NombreProducto = productoEditado.NombreProducto;
                ProductoExistente.SKU = productoEditado.SKU;
                ProductoExistente.TipoProducto = productoEditado.TipoProducto;
                ProductoExistente.ImagenUrl = productoEditado.ImagenUrl;
                // Guardar los cambios en la base de datos
                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto editado exitosamente", id = id });
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new {message = "Error al editar el producto", detalle = ex.Message });
            }
        }
        //Endpoint Delete: Ruta que seguira REACT para eliminar el producto
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarProducto(int id)
        {
            //Extraer datos para verificar que el id de usuario y lograr eliminar el producto
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if(!int.TryParse(userIdString, out int userIdToken))
            {
                return Unauthorized(new { message = "Token invalido" });
            }


            //Por seguridad validamos que el ID del producto a eliminar sea valido (mayor a 0)
            if (id <= 0)
            {
                return BadRequest(new { message = "ID del producto no valido" });

            }

            try
            {
                //Buscar el producto si existe en la base de datos
                var productoExistente = await _context.Catalogo_Productos.FindAsync(id);
                if (productoExistente == null)
                {
                    return NotFound(new { message = "Producto no encontrado", id = id });
                }

                //Control de propiedad(OWNERSHIP)
                //Comparacion el dueño del producto con el token
                if(productoExistente.UsuarioId!= userIdToken && !User.IsInRole("Admin"))
                {
                    //Retorna un status 403 Forbidden si el usuario no es el dueño del producto
                    return StatusCode(403, new { message = "No tienes permiso para eliminar este producto" });
                }
                _context.Catalogo_Productos.Remove(productoExistente);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto eliminado exitosamente", id = id });
            }
            catch (Exception ex) 
            {
                return BadRequest( new { message = "Error al eliminar el producto", detalle = ex.Message });
            }

        }
    }
}
