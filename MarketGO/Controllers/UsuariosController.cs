using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MarketGO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IMongoCollection<Usuarios> _usuariosCollection;
        private readonly IConfiguration _config;

        public UsuariosController(IMongoDatabase database, IConfiguration config)
        {
            _usuariosCollection = database.GetCollection<Usuarios>("usuarios");
            _config = config;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var listaUsuarios = await _usuariosCollection
                .Find(Builders<Usuarios>.Filter.Empty)
                .Project(u => new
                {
                    u.Id,
                    u.NombreUsuario,
                    u.Correo,
                    u.Rol,
                    u.AvatarUrl,
                    u.FechaRegistro
                })
                .ToListAsync();
            return Ok(listaUsuarios);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuarioPorId(string id)
        {
            var usuario = await _usuariosCollection.Find(u => u.Id == id).FirstOrDefaultAsync();

            if (usuario == null) return NotFound(new { message = "Usuario no encontrado" });

            return Ok(new
            {
                usuario.Id,
                usuario.NombreUsuario,
                usuario.Correo,
                usuario.Rol,
                usuario.AvatarUrl,
                usuario.Favoritos,
                usuario.FechaRegistro
            });
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(RegistroUsuarioDTO nuevoUsuario)
        {
            try
            {
                var usuarioConEmail = await _usuariosCollection.Find(u => u.Correo == nuevoUsuario.Correo).FirstOrDefaultAsync();
                if (usuarioConEmail != null) return BadRequest(new { message = "Ya existe este correo para un usuario, prueba con otro." });

                var usuarioConNombre = await _usuariosCollection.Find(u => u.NombreUsuario == nuevoUsuario.NombreUsuario).FirstOrDefaultAsync();
                if (usuarioConNombre != null) return BadRequest(new { message = "Este nombre de usuario ya está en uso. Elige otro." });

                var nuevoUsuarioBD = new Usuarios
                {
                    NombreUsuario = nuevoUsuario.NombreUsuario,
                    Correo = nuevoUsuario.Correo,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.Password),
                    Rol = "Cliente",
                    AvatarUrl = "",
                    Favoritos = new List<string>(),
                    FechaRegistro = DateTime.Now
                };

                await _usuariosCollection.InsertOneAsync(nuevoUsuarioBD);
                return Ok(new { message = "El Usuario fue creado con exito", id = nuevoUsuarioBD.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Ocurrio un error al crear el usuario", error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO credenciales)
        {
            var usuarioEncontrado = await _usuariosCollection.Find(u => u.Correo == credenciales.Correo).FirstOrDefaultAsync();
            if (usuarioEncontrado == null) return BadRequest(new { message = "El correo o contraseña son incorrectas." });

            bool passwordValida = BCrypt.Net.BCrypt.Verify(credenciales.Password, usuarioEncontrado.PasswordHash);
            if (!passwordValida) return BadRequest(new { message = "Correo o contraseña incorrectas." });

            var misCredenciales = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioEncontrado.Id),
                new Claim(ClaimTypes.Name, usuarioEncontrado.NombreUsuario),
                new Claim(ClaimTypes.Role, usuarioEncontrado.Rol ?? "Cliente")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(misCredenciales),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            return Ok(new
            {
                token = tokenString,
                usuario = new
                {
                    usuarioEncontrado.Id,
                    usuarioEncontrado.NombreUsuario,
                    usuarioEncontrado.Rol,
                    usuarioEncontrado.AvatarUrl
                }
            });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuario(string id, UsuarioEdicionDTO usuarioEditado)
        {
            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdToken)) return Unauthorized(new { message = "Token invalido" });

            var filter = Builders<Usuarios>.Filter.Eq(u => u.Id, id);
            var ExisteUsuario = await _usuariosCollection.Find(filter).FirstOrDefaultAsync();

            if (ExisteUsuario == null) return NotFound(new { message = "No se encontro el Usuario" });
            if (ExisteUsuario.Id != userIdToken && !User.IsInRole("Admin")) return StatusCode(403, new { message = "No tienes permiso para editar este usuario" });

            var update = Builders<Usuarios>.Update
                .Set(u => u.NombreUsuario, usuarioEditado.NombreUsuario)
                .Set(u => u.Correo, usuarioEditado.Correo);

            // Si el DTO incluye AvatarUrl, se actualiza (asegúrate de agregar AvatarUrl a tu UsuarioEdicionDTO en C#)
            if (!string.IsNullOrEmpty(usuarioEditado.AvatarUrl))
            {
                update = update.Set(u => u.AvatarUrl, usuarioEditado.AvatarUrl);
            }

            await _usuariosCollection.UpdateOneAsync(filter, update);
            return Ok(new { message = "El usuario fue editado con exito" });
        }

        // NUEVO: Endpoint para alternar favoritos
        [Authorize]
        [HttpPost("{id}/favoritos/{publicacionId}")]
        public async Task<IActionResult> ToggleFavorito(string id, string publicacionId)
        {
            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id != userIdToken) return StatusCode(403, new { message = "Acceso denegado" });

            var usuario = await _usuariosCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (usuario == null) return NotFound();

            var favoritos = usuario.Favoritos ?? new List<string>();
            if (favoritos.Contains(publicacionId))
            {
                favoritos.Remove(publicacionId);
            }
            else
            {
                favoritos.Add(publicacionId);
            }

            var update = Builders<Usuarios>.Update.Set(u => u.Favoritos, favoritos);
            await _usuariosCollection.UpdateOneAsync(u => u.Id == id, update);

            return Ok(new { message = "Favoritos actualizados", favoritos });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarUsuario(string id)
        {
            var userIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == userIdToken) return BadRequest(new { message = "No puedes eliminar tu propia cuenta de administrador." });

            var filter = Builders<Usuarios>.Filter.Eq(u => u.Id, id);
            var result = await _usuariosCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0) return NotFound(new { message = "El usuario no fue encontrado" });

            return Ok(new { message = "El Usuario ha sido eliminado" });
        }
    }
}