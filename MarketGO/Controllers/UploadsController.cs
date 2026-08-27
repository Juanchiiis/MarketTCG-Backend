using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protegido: solo usuarios con token pueden subir fotos
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        // Inyectamos IWebHostEnvironment para saber dónde está la carpeta física del servidor
        public UploadsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("imagen")]
        public async Task<IActionResult> SubirImagen(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new { message = "No se envió ningún archivo." });
            }

            // 1. Validar tamaño (máximo 5MB para no saturar el servidor)
            if (archivo.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "La imagen es demasiado pesada. Máximo 5MB." });
            }

            // 2. Validar que realmente sea una imagen por su extensión
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(archivo.FileName).ToLower();

            if (!extensionesPermitidas.Contains(extension))
            {
                return BadRequest(new { message = "Formato no válido. Solo se permite JPG, PNG o WEBP." });
            }

            // 3. Crear un nombre único para evitar que dos imágenes se sobreescriban
            var nombreUnico = Guid.NewGuid().ToString() + extension;

            // 4. Definir la ruta física donde se guardará (carpeta wwwroot/uploads)
            var rutaCarpeta = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");

            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }

            var rutaFisica = Path.Combine(rutaCarpeta, nombreUnico);

            // 5. Guardar el archivo físicamente en el disco duro
            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // 6. Devolver la ruta estática que React usará para mostrar la imagen y guardar en la BD
            var rutaEstatica = $"/uploads/{nombreUnico}";

            return Ok(new
            {
                message = "Imagen subida con éxito",
                url = rutaEstatica
            });
        }
    }
}