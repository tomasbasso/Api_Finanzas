using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_Finanzas.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasGastoController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public CategoriasGastoController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.CategoriasGasto
                .Select(c => new CategoriaGastoDto
                {
                    CategoriaGastoId = c.CategoriaGastoId,
                    Nombre = c.Nombre,
                    UsuarioId = c.UsuarioId
                }).ToListAsync();

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoria(int id)
        {
            var categoria = await _context.CategoriasGasto.FindAsync(id);
            if (categoria == null)
                return NotFound();

            var dto = new CategoriaGastoDto
            {
                CategoriaGastoId = categoria.CategoriaGastoId,
                Nombre = categoria.Nombre,
                UsuarioId = categoria.UsuarioId
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaGastoDto dto)
        {
           
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var nueva = new CategoriaGasto
            {
                Nombre = dto.Nombre,
                UsuarioId = userId    
            };

            _context.CategoriasGasto.Add(nueva);
            await _context.SaveChangesAsync();

            var resultado = new CategoriaGastoDto
            {
                CategoriaGastoId = nueva.CategoriaGastoId,
                Nombre = nueva.Nombre,
                UsuarioId = nueva.UsuarioId
            };

            return CreatedAtAction(nameof(GetCategoria),
                                   new { id = nueva.CategoriaGastoId },
                                   resultado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCategoria(int id, CategoriaGastoDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var categoria = await _context.CategoriasGasto.FindAsync(id);
            if (categoria == null)
                return NotFound();

            categoria.Nombre = dto.Nombre;
            categoria.UsuarioId = userId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.CategoriasGasto.FindAsync(id);
            if (categoria == null)
                return NotFound();

            _context.CategoriasGasto.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
