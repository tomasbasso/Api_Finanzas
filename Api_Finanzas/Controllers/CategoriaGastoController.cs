using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_Finanzas.Controllers
{
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
        public async Task<IActionResult> CrearCategoria(CategoriaGastoDto dto)
        {
            var categoria = new CategoriaGasto
            {
                Nombre = dto.Nombre,
                UsuarioId = dto.UsuarioId
            };

            _context.CategoriasGasto.Add(categoria);
            await _context.SaveChangesAsync();

            dto.CategoriaGastoId = categoria.CategoriaGastoId;
            return CreatedAtAction(nameof(GetCategoria), new { id = dto.CategoriaGastoId }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCategoria(int id, CategoriaGastoDto dto)
        {
            var categoria = await _context.CategoriasGasto.FindAsync(id);
            if (categoria == null)
                return NotFound();

            categoria.Nombre = dto.Nombre;
            categoria.UsuarioId = dto.UsuarioId;

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
