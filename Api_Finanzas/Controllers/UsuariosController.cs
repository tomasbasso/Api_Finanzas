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

            // Por ahora no devolvemos token
            return Ok(new UsuarioDto
            {
                UsuarioId = usuario.UsuarioId,
                Nombre = usuario.Nombre,
                Email = usuario.Email
            });
        }


        //[Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(claims);
        }



    }
}
