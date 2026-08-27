using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicacionesInventarioController : ControllerBase
    {
        private readonly MarketGOContext _context;

        //El constructor recibe el contexto de la base de datos para poder usarlo en los métodos
        public PublicacionesInventarioController(MarketGOContext context)
        {
            _context = context;
        }

        //Endpoint GET : La ruta que REACT llamara para obtener la lista de publicaciones de inventario

        [HttpGet]
        public async Task<IActionResult> GetPublicacionInventari()
        {
            //Usamos Query Syntax para unir TRES tablas sin complicación
            var consultas = from publicacion in _context.Publicaciones_Inventarios
                                //Primer JOIN: Conectamos el inventario con el codigo de productos
                                //Ojo : Asegurate  de que en tu clase la propiedad se llama ProductoId
                            join producto in _context.Catalogo_Productos
                            on publicacion.ProductoCatalogoId equals producto.Id

                            //Segundo JOIN : Conectamos el inventario con el usuario 

                            join usuario in _context.Usuarios
                            on publicacion.VendedorId equals usuario.Id

                            //Volcamos todo al DTO combinando los datos
                            select new PublicacionInventarioDTO
                            {
                                Id = publicacion.Id,
                                ProductoCatalogoId = publicacion.ProductoCatalogoId,
                                ColeccionId = producto.ColeccionId,
                                SKU = producto.SKU,
                                TipoProducto = producto.TipoProducto,
                                NombreProducto = producto.NombreProducto,
                                ImagenUrl = producto.ImagenUrl,
                                NombreVendedor = usuario.NombreUsuario,
                                Estado = publicacion.Estado,
                                Precio = publicacion.Precio,
                                Notas = publicacion.Notas
                            };
            //Ejecutamos la consulta en la base de datos y la convertimos en lista 
            var listaPublicacionInventario = await consultas.ToListAsync();
            return Ok(listaPublicacionInventario);
        }

        // Endpoint GET: Trae SOLO las publicaciones del usuario logueado
        [Authorize]
        [HttpGet("MisPublicaciones")]
        public async Task<IActionResult> GetMisPublicaciones()
        {
            try
            {
                // 1. Sacamos la ID del token del usuario que inició sesión
                var claimID = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (claimID == null) return Unauthorized(new { message = "Token inválido" });

                int idUsuarioLogueado = int.Parse(claimID);

                // 2. Hacemos el mismo JOIN de antes, pero filtramos por el VendedorId
                var misPublicaciones = await (from publicacion in _context.Publicaciones_Inventarios
                                              join producto in _context.Catalogo_Productos
                                              on publicacion.ProductoCatalogoId equals producto.Id
                                              join usuario in _context.Usuarios
                                              on publicacion.VendedorId equals usuario.Id
                                              where publicacion.VendedorId == idUsuarioLogueado // <-- EL FILTRO
                                              select new PublicacionInventarioDTO
                                              {
                                                  Id = publicacion.Id,
                                                  ProductoCatalogoId = publicacion.ProductoCatalogoId,
                                                  ColeccionId = producto.ColeccionId,
                                                  SKU = producto.SKU,
                                                  TipoProducto = producto.TipoProducto,
                                                  NombreProducto = producto.NombreProducto,
                                                  ImagenUrl = producto.ImagenUrl,
                                                  NombreVendedor = usuario.NombreUsuario,
                                                  Estado = publicacion.Estado,
                                                  Precio = publicacion.Precio,
                                                  Notas = publicacion.Notas
                                              }).ToListAsync();

                return Ok(misPublicaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener tus publicaciones", error = ex.Message });
            }
        }
        // Endpoint GET: Buscador Avanzado con Filtros Dinámicos y Paginación
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarPublicaciones(
            [FromQuery] string? nombre,
            [FromQuery] string? franquicia,
            [FromQuery] string? coleccion,
            [FromQuery] decimal? precioMin,
            [FromQuery] decimal? precioMax,
            [FromQuery] int pagina = 1,
            [FromQuery] int limite = 10)
        {
            try
            {
                // 1. Unimos las 5 tablas necesarias (Inventario, Producto, Coleccion, Franquicia, Usuario)
                // Usamos AsQueryable() para no ejecutar la consulta todavía en la BD
                var query = (from publicacion in _context.Publicaciones_Inventarios
                             join producto in _context.Catalogo_Productos on publicacion.ProductoCatalogoId equals producto.Id
                             join col in _context.Colecciones on producto.ColeccionId equals col.Id
                             join franq in _context.Franquicias on col.FranquiciaId equals franq.Id
                             join usuario in _context.Usuarios on publicacion.VendedorId equals usuario.Id
                             where publicacion.Stock > 0 // Solo mostramos lo que se puede comprar
                             select new
                             {
                                 publicacion,
                                 producto,
                                 col,
                                 franq,
                                 usuario
                             }).AsQueryable();

                // 2. Aplicamos filtros dinámicos solo si el frontend los envió
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    query = query.Where(q => q.producto.NombreProducto.Contains(nombre));
                }

                if (!string.IsNullOrWhiteSpace(franquicia))
                {
                    query = query.Where(q => q.franq.Nombre.Contains(franquicia));
                }

                if (!string.IsNullOrWhiteSpace(coleccion))
                {
                    query = query.Where(q => q.col.NombreColeccion.Contains(coleccion));
                }

                if (precioMin.HasValue)
                {
                    query = query.Where(q => q.publicacion.Precio >= precioMin.Value);
                }

                if (precioMax.HasValue)
                {
                    query = query.Where(q => q.publicacion.Precio <= precioMax.Value);
                }

                // 3. Calculamos la paginación
                int totalItems = await query.CountAsync(); // Cuenta cuántos resultados hay en total
                int totalPaginas = (int)Math.Ceiling(totalItems / (double)limite);

                // 4. Aplicamos el Skip, Take y armamos el DTO final
                var resultados = await query
                    .OrderByDescending(q => q.publicacion.Id) // Ordenar por los más nuevos
                    .Skip((pagina - 1) * limite)
                    .Take(limite)
                    .Select(q => new PublicacionInventarioDTO
                    {
                        Id = q.publicacion.Id,
                        ProductoCatalogoId = q.publicacion.ProductoCatalogoId,
                        ColeccionId = q.producto.ColeccionId,
                        SKU = q.producto.SKU,
                        TipoProducto = q.producto.TipoProducto,
                        NombreProducto = q.producto.NombreProducto,
                        NombreColeccion = q.col.NombreColeccion, // Nuevo
                        NombreFranquicia = q.franq.Nombre,       // Nuevo
                        ImagenUrl = q.producto.ImagenUrl,
                        NombreVendedor = q.usuario.NombreUsuario,
                        Estado = q.publicacion.Estado,
                        Precio = q.publicacion.Precio,
                        Notas = q.publicacion.Notas
                    })
                    .ToListAsync(); // <-- ¡Aquí es donde C# finalmente viaja a SQL Server!

                // 5. Envolvemos la respuesta para el Frontend
                var respuesta = new RespuestaPaginadaDTO<PublicacionInventarioDTO>
                {
                    TotalItems = totalItems,
                    TotalPaginas = totalPaginas,
                    PaginaActual = pagina,
                    Items = resultados
                };

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar productos", error = ex.Message });
            }
        }

        //Endpoint Post : La ruta que REACT llamara para crear lista de publicaciones de inventario
        [Authorize]
        [HttpPost]
        public async Task<IActionResult>CrearPublicacionInventario([FromBody] PublicacionInventarioCreacionDTO nuevaPublicacionInventario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var claimID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (claimID == null) return Unauthorized(new { message = "Token invalido o sin permisos" });

                int idUsuarioLogueado = int.Parse(claimID);

                var publicacion = new PublicacionInventario
                {
                    ProductoCatalogoId = nuevaPublicacionInventario.ProductoCatalogoId,
                    VendedorId = idUsuarioLogueado,
                    Estado = nuevaPublicacionInventario.Estado,
                    Precio = nuevaPublicacionInventario.Precio,
                    Stock = nuevaPublicacionInventario.Stock,
                    Notas = nuevaPublicacionInventario.Notas
                };

                _context.Publicaciones_Inventarios.Add(publicacion);
               await _context.SaveChangesAsync();

                return Ok(new { message = "Se ha creado con exito la publicación de inventario" });
            }
            catch (Exception ex)
            { 
                return BadRequest(new { message = "Error al crear la publicación de inventario", error = ex.Message });
            }
        }

        //Enpoint PUT: La ruta que seguira REACT para editar la lista de pubicaciones de inventario
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarPublicacionInventario(int id, [FromBody] PublicacionInventarioCreacionDTO editarPublicacionInventario)
        {
            //Preguntamos si el id de Publicacion de inventario es mayor a 0
            if (id <= 0 && !User.IsInRole("Admin"))
            {
                return BadRequest(new { message = "El ID es invalido" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                var claimID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (claimID == null) return Unauthorized(new { message = "Token invalido" });
                int idUsariologueado = int.Parse(claimID);

                //Validamos si el id existe
                var ExistePublicacionesInventario = await _context.Publicaciones_Inventarios.FindAsync(id);
                if (ExistePublicacionesInventario == null)
                {
                    return NotFound(new { message = "La id no existe", id = id });
                }

                if(ExistePublicacionesInventario.VendedorId != idUsariologueado)
                {
                    return StatusCode(403, new { message = "Acceso denegado, no puedes editar una publicacion que no es tuya." });
                }
                //Actualizamos los campos con los nuevos valores
                //Id no se modifica por seguridad
                ExistePublicacionesInventario.ProductoCatalogoId = editarPublicacionInventario.ProductoCatalogoId;
                ExistePublicacionesInventario.Estado = editarPublicacionInventario.Estado;
                ExistePublicacionesInventario.Precio = editarPublicacionInventario.Precio;
                ExistePublicacionesInventario.Stock = editarPublicacionInventario.Stock;
                ExistePublicacionesInventario.Notas = editarPublicacionInventario.Notas;

                await _context.SaveChangesAsync();
                return Ok(new { message = "La Publicacion de inventarios se edito correctamente" });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "Error al editar la Publicacion de inventarios", error = ex.Message });
            }

        }

        //Endpoint Delete: La ruta que seguira REACT para eliminar la lista de publicaciones de inventario
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPublicacionInventario (int id)
        {
            //Preguntamos si la id del producto es mayor a 0
            if (id <= 0 && !User.IsInRole("Admin"))
            {
                return BadRequest(new { message = "La ID es invalida" });
            }
            try
            {
                var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (claimId == null) return Unauthorized(new { message = "Token invalido " });

                int idUsuarioLogueado = int.Parse(claimId);

                //Preguntar si el producto existe en la base de datos
                var ExistePublicacionInventario = await _context.Publicaciones_Inventarios.FindAsync(id);
                if (ExistePublicacionInventario== null)
                {
                    return NotFound(new { message = "La Publicacion de inventario no fue encontrada en la base de datos", id = id });
                }

                if(ExistePublicacionInventario.VendedorId != idUsuarioLogueado)
                {
                    return StatusCode(403, new { message = "Acceso denegado, no puedes eliminar una publicacion que no es tuya." });
                }

                _context.Publicaciones_Inventarios.Remove(ExistePublicacionInventario);
                await _context.SaveChangesAsync();
                return Ok(new { message = "La publicación de inventario fue borrada con exitoo!!!" });
            }
            catch (Exception ex) 
            {
                return BadRequest(new { message = "Error al eliminar la publicación de inventario", error = ex.Message });
            }

        }


    } 

}