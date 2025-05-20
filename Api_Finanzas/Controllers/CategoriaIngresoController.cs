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
    public class CategoriaIngresoController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public CategoriaIngresoController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categorias = await _context.CategoriasIngreso
                .Select(c => new CategoriaIngresoDto
                {
                    CategoriaIngresoId = c.CategoriaIngresoId,
                    Nombre = c.Nombre,
                    UsuarioId = c.UsuarioId
                })
                .ToListAsync();

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var categoria = await _context.CategoriasIngreso
                .Where(c => c.CategoriaIngresoId == id)
                .Select(c => new CategoriaIngresoDto
                {
                    CategoriaIngresoId = c.CategoriaIngresoId,
                    Nombre = c.Nombre,
                    UsuarioId = c.UsuarioId
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
                return NotFound();

            return Ok(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoriaIngresoCreateDto dto)
        {
            var nueva = new CategoriaIngreso
            {
                Nombre = dto.Nombre,
                UsuarioId = dto.UsuarioId
            };

            _context.CategoriasIngreso.Add(nueva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = nueva.CategoriaIngresoId }, new CategoriaIngresoDto
            {
                CategoriaIngresoId = nueva.CategoriaIngresoId,
                Nombre = nueva.Nombre,
                UsuarioId = nueva.UsuarioId
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoriaIngresoCreateDto dto)
        {
            var categoria = await _context.CategoriasIngreso.FindAsync(id);
            if (categoria == null)
                return NotFound();

            categoria.Nombre = dto.Nombre;
            categoria.UsuarioId = dto.UsuarioId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.CategoriasIngreso.FindAsync(id);
            if (categoria == null)
                return NotFound();

            _context.CategoriasIngreso.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
