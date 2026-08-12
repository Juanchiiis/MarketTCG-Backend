using MarketGO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly MarketGOContext _context;
        private readonly IConfiguration _config; 

        //El constructor recibe el contexto de la base de datos para poder usarlo en los métodos
        public UsuariosController(MarketGOContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        //Endpoint GET : La ruta que REACT llamara para obtener la lista de usuarios
        [Authorize (Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            //Usamos .Select()  para transoformar cada entidad (con contraseña)
            //en un DTO (sin contraseña antes de mandarlo)
            var listaUsuarios = await _context.Usuarios.Select(usuariosBD => new UsuarioDTO
                {
                    Id = usuariosBD.Id,
                    NombreUsuario = usuariosBD.NombreUsuario,
                    Correo = usuariosBD.Correo,
                    Rol = usuariosBD.Rol,
                    FechaRegistro = usuariosBD.FechaRegistro
                //Como el DTO no tiene donde guardar la contraseña, simplemente no la extraemos de 'usuarioBD'.
            }

            ).ToListAsync();
            return Ok(listaUsuarios);
        }
        //Enpoint Post : La ruta que seguira REACT para obtener la lista de usuarios
        
        [HttpPost]
        public async Task<IActionResult> CrearUsuario(RegistroUsuarioDTO nuevoUsuario)
        {
            try
            {
                var nuevoUsuarioBD = new Usuarios
                {
                    NombreUsuario = nuevoUsuario.NombreUsuario,
                    Correo = nuevoUsuario.Correo,
                    //toma la clave del DTO y la guarda en el hash de la base de datos
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevoUsuario.Password),
                    Rol = "Cliente",
                    FechaRegistro = DateTime.Now //Se asigna la fecha actual al momento de crear el usuario
                };
                
                _context.Usuarios.Add(nuevoUsuarioBD);


                //Guarda el nuevo usuario en la base de datos
                await _context.SaveChangesAsync();
                return Ok(new { message = "El Usuario fue creado con exito", id = nuevoUsuarioBD.Id });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = "Ocurrio un error al crear el usuario", error = ex.Message });
            }
        }

        //Endpoint LOGIN Crear Credencial
        
        [HttpPost("login")] //La ruta es /api/Usuarios/login
        public async Task<IActionResult> Login(LoginDTO credenciales)
        {
            //1. Buscamos si el correo existe en la base de datos
            var usuarioEncontrado = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == credenciales.Correo);
            if (usuarioEncontrado == null)
            {
                return BadRequest(new { message = "El correo o contraseña son incorrectas." });
            }
            //2. Comparamos la contraseña escrita con el Hash
            bool passwordValida = BCrypt.Net.BCrypt.Verify(credenciales.Password, usuarioEncontrado.PasswordHash);
            
            if (!passwordValida)
            {
                return BadRequest(new { message = "Correo o contraseña incorrectas." });
            }

            var misCredenciales = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioEncontrado.Id.ToString()),
                new Claim(ClaimTypes.Name, usuarioEncontrado.NombreUsuario),
                new Claim(ClaimTypes.Role, usuarioEncontrado.Rol ?? "Cliente")
            };

            var stringKey = _config["Jwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(stringKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(misCredenciales),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = creds
            };

            // 6. El manejador escribe el token final en un string largo
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenObjeto = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(tokenObjeto);

            // 7. Le respondemos a React con el Token de acceso y los datos básicos
            return Ok(new
            {
                token = tokenString,
                usuario = new { usuarioEncontrado.Id, usuarioEncontrado.NombreUsuario, usuarioEncontrado.Rol }
            });
        }


        //Enpoint PUT: La ruta que seguira REACT para editar la lista de usuarios
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuario(int id, UsuarioEdicionDTO usuarioEditado)
        {
            //Extraerdatos para verificar que el id de usuario y lograr editar el usuario}
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if(!int.TryParse(userIdString, out int userIdToken))
            {
                return Unauthorized(new { message = "Token invalido " });
            }

            //Preguntamos si la id es mayor a 0
            if (id <= 0)
            {
                return BadRequest(new { message = "La ID es invalida" });
            }
            try
            {


                //Preguntamos si el id existe en la base de datos
                var ExisteUsuario = await _context.Usuarios.FindAsync(id);
                if (ExisteUsuario == null)
                {
                    return NotFound(new { message = "No se encontro el Usuario en la base de datos", id = id });
                }

                //Control de propiedad(OWNERSHIP)
                //Comparacion el dueño de la cuenta con el token
                if (ExisteUsuario.Id != userIdToken && !User.IsInRole("Admin"))
                {
                    return StatusCode(403, new { message = "No tienes permiso para editar este usuario" });
                }

                //Actualizamos solo la informacion segura mandada desde el dto
                ExisteUsuario.NombreUsuario = usuarioEditado.NombreUsuario;
                ExisteUsuario.Correo = usuarioEditado.Correo;



                await _context.SaveChangesAsync();

                return Ok(new { message = "El usuario fue editado con exito " });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrio un error al editar el usuario", error = ex.Message });
            }
        }

        //Endpoint Delete : Ruta que seguira REACT para eliminar el usuario
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarUsuario(int id)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int userIdToken = 0;
            try
            {
                //Validamos si el id de usuario es mayoe a 0
                if (!int.TryParse(userIdString, out userIdToken))
                {
                    return Unauthorized(new { message = "La id de usuario no fue encontrada"});
                }
                //Validamos si el id de usuario existe en la base de datos
                var ExisteUsuario = await _context.Usuarios.FindAsync(id);
                if (ExisteUsuario == null)
                {
                    return NotFound(new { message = "El usuario no fue encontrado en la base de datos", id = id });
                }
                if (id == userIdToken)
                {
                    return BadRequest(new { message = "No puedes eliminar tu propia cuenta de administrador." });
                }

                _context.Usuarios.Remove(ExisteUsuario);

                await _context.SaveChangesAsync();

                return Ok(new { message = "El Usuario ha sido eliminado!!" });
            } catch (Exception ex)
            {
                return BadRequest(new { message = "Ocurrio un error al eliminar el usuario", error = ex.Message });          
            }
        }
    }
}
