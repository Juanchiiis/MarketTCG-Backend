using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly IMongoCollection<CatalogoProducto> _productosCollection;

        public ProductosController(IMongoDatabase database)
        {
            _productosCollection = database.GetCollection<CatalogoProducto>("catalogo_productos");
        }

        // Endpoint para crear el producto en el catálogo (El famoso PASO 1)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoCreacionDTO nuevoProducto)
        {
            try
            {
                var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdToken)) return Unauthorized(new { message = "Token inválido" });

                var productoBD = new CatalogoProducto
                {
                    ColeccionId = nuevoProducto.ColeccionId,
                    NombreProducto = nuevoProducto.NombreProducto,
                    SKU = nuevoProducto.SKU,
                    TipoProducto = nuevoProducto.TipoProducto,
                    ImagenUrl = nuevoProducto.ImagenUrl, // Aquí se guarda la URL de ImgBB
                    UsuarioId = userIdToken
                };

                await _productosCollection.InsertOneAsync(productoBD);

                // DEVOLVEMOS EL ID EXACTO QUE ESPERA EL FRONTEND
                return Ok(new
                {
                    message = "Producto creado en el catálogo exitosamente",
                    id = productoBD.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear el producto", detalle = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _productosCollection.Find(Builders<CatalogoProducto>.Filter.Empty).ToListAsync();
            return Ok(productos);
        }
    }

    // Asegúrate de tener este DTO en tu carpeta Models o aquí mismo
    public class ProductoCreacionDTO
    {
        public string ColeccionId { get; set; } = string.Empty;
        public string NombreProducto { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string TipoProducto { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
    }
}