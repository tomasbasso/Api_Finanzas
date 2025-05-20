using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Api_Finanzas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Claims;

namespace Api_Finanzas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly FinanzasDbContext _context;
        
        private readonly IConfiguration _configuration;
        public UsuariosController(FinanzasDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
      

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (_context.Usuarios.Any(u => u.Email == dto.Email))
                return BadRequest("El email ya está registrado.");

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena),
                Rol = dto.Rol
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Contrasena, usuario.ContrasenaHash))
                return Unauthorized("Credenciales inválidas");

            var jwtService = new JwtService(_configuration);
            var token = jwtService.GenerarToken(usuario);

            return Ok(new
            {
                Token = token,
                Usuario = new UsuarioDto
                {
                    UsuarioId = usuario.UsuarioId,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol
                }
            });
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new UsuarioDto
                {
                    UsuarioId = u.UsuarioId,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Rol = u.Rol
                })
                .ToListAsync();

            return Ok(usuarios);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuarioDto(int id, EditarUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            if (!string.IsNullOrEmpty(dto.Nombre))
                usuario.Nombre = dto.Nombre;

            if (!string.IsNullOrEmpty(dto.Rol))
                usuario.Rol = dto.Rol;

            // Solo actualiza el hash si la contraseña fue enviada
            if (!string.IsNullOrEmpty(dto.Contrasena))
                usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena);

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Ok(new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol
            });
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Cuentas)
                    .ThenInclude(c => c.Transacciones)
                .Include(u => u.Transacciones)
                .Include(u => u.Presupuestos)
                .Include(u => u.MetasAhorro)
                .Include(u => u.Alertas)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            // 1. Borrá las transacciones asociadas a las cuentas bancarias del usuario
            foreach (var cuenta in usuario.Cuentas)
            {
                if (cuenta.Transacciones != null && cuenta.Transacciones.Any())
                    _context.Transacciones.RemoveRange(cuenta.Transacciones);
            }

            // 2. Borrá las cuentas bancarias
            if (usuario.Cuentas != null && usuario.Cuentas.Any())
                _context.CuentasBancarias.RemoveRange(usuario.Cuentas);

            // 3. Borrá otras transacciones directas del usuario (si hay)
            if (usuario.Transacciones != null && usuario.Transacciones.Any())
                _context.Transacciones.RemoveRange(usuario.Transacciones);

            // 4. Borrá presupuestos
            if (usuario.Presupuestos != null && usuario.Presupuestos.Any())
                _context.Presupuestos.RemoveRange(usuario.Presupuestos);

            // 5. Borrá metas de ahorro
            if (usuario.MetasAhorro != null && usuario.MetasAhorro.Any())
                _context.MetasAhorro.RemoveRange(usuario.MetasAhorro);

            // 6. Borrá alertas
            if (usuario.Alertas != null && usuario.Alertas.Any())
                _context.Alertas.RemoveRange(usuario.Alertas);

            // 7. Finalmente, borrá el usuario
            _context.Usuarios.Remove(usuario);

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest("No se pudo eliminar el usuario: " + ex.ToString());
            }
        }


        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var email = SesionActual.Email;

            if (string.IsNullOrEmpty(email))
                return Unauthorized("No hay usuario autenticado");

            var usuario = await _context.Usuarios
                .Where(u => u.Email == email)
                .Select(u => new UsuarioDto
                {
                    UsuarioId = u.UsuarioId,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Rol = u.Rol
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            return Ok(usuario);
        }
    }
    }
