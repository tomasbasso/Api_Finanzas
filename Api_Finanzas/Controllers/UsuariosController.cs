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
        //private readonly IJwtService _jwtService;

        public UsuariosController(FinanzasDbContext context)
        {
            _context = context;
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
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Email = usuario.Email
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Contrasena, usuario.ContrasenaHash))
                return Unauthorized("Credenciales inválidas");

            // Simulamos una sesión guardando el email
            SesionActual.Email = usuario.Email;

            return Ok(new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Email = usuario.Email
            });
        }


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
                    Email = u.Email
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            return Ok(usuario);
        }
    }
    }
