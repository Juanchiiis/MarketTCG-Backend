using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicacionesInventarioController : ControllerBase
    {
        private readonly IMongoCollection<PublicacionInventario> _publicacionesCollection;
        private readonly IMongoCollection<CatalogoProducto> _productosCollection;
        private readonly IMongoCollection<Usuarios> _usuariosCollection;
        private readonly IMongoCollection<Colecciones> _coleccionesCollection;
        private readonly IMongoCollection<Franquicias> _franquiciasCollection;

        public PublicacionesInventarioController(IMongoDatabase database)
        {
            _publicacionesCollection = database.GetCollection<PublicacionInventario>("publicaciones_inventarios");
            _productosCollection = database.GetCollection<CatalogoProducto>("catalogo_productos");
            _usuariosCollection = database.GetCollection<Usuarios>("usuarios");
            _coleccionesCollection = database.GetCollection<Colecciones>("colecciones");
            _franquiciasCollection = database.GetCollection<Franquicias>("franquicias");
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicacionInventari()
        {
            var publicaciones = await _publicacionesCollection.Find(Builders<PublicacionInventario>.Filter.Empty).ToListAsync();
            var productos = await _productosCollection.Find(Builders<CatalogoProducto>.Filter.Empty).ToListAsync();
            var usuarios = await _usuariosCollection.Find(Builders<Usuarios>.Filter.Empty).ToListAsync();

            var listaPublicacionInventario = publicaciones.Select(publicacion =>
            {
                var producto = productos.FirstOrDefault(p => p.Id == publicacion.ProductoCatalogoId);
                var usuario = usuarios.FirstOrDefault(u => u.Id == publicacion.VendedorId);

                return new PublicacionInventarioDTO
                {
                    Id = publicacion.Id,
                    ProductoCatalogoId = publicacion.ProductoCatalogoId,
                    ColeccionId = producto?.ColeccionId ?? string.Empty,
                    SKU = producto?.SKU ?? string.Empty,
                    TipoProducto = producto?.TipoProducto ?? string.Empty,
                    NombreProducto = producto?.NombreProducto ?? "Desconocido",
                    ImagenUrl = producto?.ImagenUrl ?? string.Empty,
                    NombreVendedor = usuario?.NombreUsuario ?? "Anónimo",
                    Estado = publicacion.Estado,
                    Precio = publicacion.Precio,
                    Notas = publicacion.Notas
                };
            }).ToList();

            return Ok(listaPublicacionInventario);
        }

        [Authorize]
        [HttpGet("MisPublicaciones")]
        public async Task<IActionResult> GetMisPublicaciones()
        {
            try
            {
                var idUsuarioLogueado = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(idUsuarioLogueado)) return Unauthorized(new { message = "Token inválido" });

                var filter = Builders<PublicacionInventario>.Filter.Eq(p => p.VendedorId, idUsuarioLogueado);
                var publicaciones = await _publicacionesCollection.Find(filter).ToListAsync();
                var productos = await _productosCollection.Find(Builders<CatalogoProducto>.Filter.Empty).ToListAsync();
                var usuarios = await _usuariosCollection.Find(Builders<Usuarios>.Filter.Empty).ToListAsync();

                var misPublicaciones = publicaciones.Select(publicacion =>
                {
                    var producto = productos.FirstOrDefault(p => p.Id == publicacion.ProductoCatalogoId);
                    var usuario = usuarios.FirstOrDefault(u => u.Id == publicacion.VendedorId);

                    return new PublicacionInventarioDTO
                    {
                        Id = publicacion.Id,
                        ProductoCatalogoId = publicacion.ProductoCatalogoId,
                        ColeccionId = producto?.ColeccionId ?? string.Empty,
                        SKU = producto?.SKU ?? string.Empty,
                        TipoProducto = producto?.TipoProducto ?? string.Empty,
                        NombreProducto = producto?.NombreProducto ?? "Desconocido",
                        ImagenUrl = producto?.ImagenUrl ?? string.Empty,
                        NombreVendedor = usuario?.NombreUsuario ?? "Anónimo",
                        Estado = publicacion.Estado,
                        Precio = publicacion.Precio,
                        Notas = publicacion.Notas
                    };
                }).ToList();

                return Ok(misPublicaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener tus publicaciones", error = ex.Message });
            }
        }

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
                var publicaciones = await _publicacionesCollection
                    .Find(Builders<PublicacionInventario>.Filter.Gt(p => p.Stock, 0))
                    .ToListAsync();

                var productos = await _productosCollection.Find(Builders<CatalogoProducto>.Filter.Empty).ToListAsync();
                var usuarios = await _usuariosCollection.Find(Builders<Usuarios>.Filter.Empty).ToListAsync();
                var colecciones = await _coleccionesCollection.Find(Builders<Colecciones>.Filter.Empty).ToListAsync();
                var franquicias = await _franquiciasCollection.Find(Builders<Franquicias>.Filter.Empty).ToListAsync();

                var resultados = publicaciones
                    .Select(pub =>
                    {
                        var producto = productos.FirstOrDefault(p => p.Id == pub.ProductoCatalogoId);
                        var usuario = usuarios.FirstOrDefault(u => u.Id == pub.VendedorId);
                        var col = colecciones.FirstOrDefault(c => c.Id == producto?.ColeccionId);
                        var franq = franquicias.FirstOrDefault(f => f.Id == col?.FranquiciaId);

                        return new
                        {
                            pub,
                            producto,
                            col,
                            franq,
                            usuario
                        };
                    })
                    .Where(x =>
                    {
                        if (!string.IsNullOrWhiteSpace(nombre) && 
                            (x.producto?.NombreProducto ?? "").IndexOf(nombre, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;

                        if (!string.IsNullOrWhiteSpace(franquicia) && 
                            (x.franq?.Nombre ?? "").IndexOf(franquicia, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;

                        if (!string.IsNullOrWhiteSpace(coleccion) && 
                            (x.col?.NombreColeccion ?? "").IndexOf(coleccion, StringComparison.OrdinalIgnoreCase) < 0)
                            return false;

                        if (precioMin.HasValue && x.pub.Precio < precioMin.Value)
                            return false;

                        if (precioMax.HasValue && x.pub.Precio > precioMax.Value)
                            return false;

                        return true;
                    })
                    .OrderByDescending(x => x.pub.Id)
                    .ToList();

                int totalItems = resultados.Count;
                int totalPaginas = (int)Math.Ceiling(totalItems / (double)limite);

                var itemsPaginados = resultados
                    .Skip((pagina - 1) * limite)
                    .Take(limite)
                    .Select(q => new PublicacionInventarioDTO
                    {
                        Id = q.pub.Id,
                        ProductoCatalogoId = q.pub.ProductoCatalogoId,
                        ColeccionId = q.producto?.ColeccionId ?? string.Empty,
                        SKU = q.producto?.SKU ?? string.Empty,
                        TipoProducto = q.producto?.TipoProducto ?? string.Empty,
                        NombreProducto = q.producto?.NombreProducto ?? "Desconocido",
                        NombreColeccion = q.col?.NombreColeccion ?? string.Empty,
                        NombreFranquicia = q.franq?.Nombre ?? string.Empty,
                        ImagenUrl = q.producto?.ImagenUrl ?? string.Empty,
                        NombreVendedor = q.usuario?.NombreUsuario ?? "Anónimo",
                        Estado = q.pub.Estado,
                        Precio = q.pub.Precio,
                        Notas = q.pub.Notas
                    })
                    .ToList();

                var respuesta = new RespuestaPaginadaDTO<PublicacionInventarioDTO>
                {
                    TotalItems = totalItems,
                    TotalPaginas = totalPaginas,
                    PaginaActual = pagina,
                    Items = itemsPaginados
                };

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al buscar productos", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CrearPublicacionInventario([FromBody] PublicacionInventarioCreacionDTO nuevaPublicacionInventario)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var idUsuarioLogueado = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(idUsuarioLogueado)) return Unauthorized(new { message = "Token invalido o sin permisos" });

                var publicacion = new PublicacionInventario
                {
                    ProductoCatalogoId = nuevaPublicacionInventario.ProductoCatalogoId,
                    VendedorId = idUsuarioLogueado,
                    Estado = nuevaPublicacionInventario.Estado,
                    Precio = nuevaPublicacionInventario.Precio,
                    Stock = nuevaPublicacionInventario.Stock,
                    Notas = nuevaPublicacionInventario.Notas
                };

                await _publicacionesCollection.InsertOneAsync(publicacion);

                return Ok(new { message = "Se ha creado con exito la publicación de inventario" });
            }
            catch (Exception ex)
            { 
                return BadRequest(new { message = "Error al crear la publicación de inventario", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarPublicacionInventario(string id, [FromBody] PublicacionInventarioCreacionDTO editarPublicacionInventario)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "El ID es invalido" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var idUsariologueado = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(idUsariologueado)) return Unauthorized(new { message = "Token invalido" });

                var filter = Builders<PublicacionInventario>.Filter.Eq(p => p.Id, id);
                var ExistePublicacionesInventario = await _publicacionesCollection.Find(filter).FirstOrDefaultAsync();

                if (ExistePublicacionesInventario == null)
                {
                    return NotFound(new { message = "La id no existe", id = id });
                }

                if(ExistePublicacionesInventario.VendedorId != idUsariologueado)
                {
                    return StatusCode(403, new { message = "Acceso denegado, no puedes editar una publicacion que no es tuya." });
                }

                var update = Builders<PublicacionInventario>.Update
                    .Set(p => p.ProductoCatalogoId, editarPublicacionInventario.ProductoCatalogoId)
                    .Set(p => p.Estado, editarPublicacionInventario.Estado)
                    .Set(p => p.Precio, editarPublicacionInventario.Precio)
                    .Set(p => p.Stock, editarPublicacionInventario.Stock)
                    .Set(p => p.Notas, editarPublicacionInventario.Notas);

                await _publicacionesCollection.UpdateOneAsync(filter, update);
                return Ok(new { message = "La Publicacion de inventarios se edito correctamente" });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "Error al editar la Publicacion de inventarios", error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPublicacionInventario(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "La ID es invalida" });
            }
            try
            {
                var idUsuarioLogueado = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(idUsuarioLogueado)) return Unauthorized(new { message = "Token invalido " });

                var filter = Builders<PublicacionInventario>.Filter.Eq(p => p.Id, id);
                var ExistePublicacionInventario = await _publicacionesCollection.Find(filter).FirstOrDefaultAsync();

                if (ExistePublicacionInventario == null)
                {
                    return NotFound(new { message = "La Publicacion de inventario no fue encontrada en la base de datos", id = id });
                }

                if(ExistePublicacionInventario.VendedorId != idUsuarioLogueado)
                {
                    return StatusCode(403, new { message = "Acceso denegado, no puedes eliminar una publicacion que no es tuya." });
                }

                await _publicacionesCollection.DeleteOneAsync(filter);
                return Ok(new { message = "La publicación de inventario fue borrada con exitoo!!!" });
            }
            catch (Exception ex) 
            {
                return BadRequest(new { message = "Error al eliminar la publicación de inventario", error = ex.Message });
            }
        }
    }
}
