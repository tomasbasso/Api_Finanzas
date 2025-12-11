using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        public async Task<IActionResult> GetAll([FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();

            var categorias = await _context.CategoriasIngreso
                .AsNoTracking()
                .Where(c => allowAdminAll || c.UsuarioId == userId)
                .Select(c => new CategoriaIngresoDto
                {
                    CategoriaIngresoId = c.CategoriaIngresoId,
                    Nombre = c.Nombre,
                    UsuarioId = c.UsuarioId
                })
                .ToListAsync(ct);

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, [FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();
            var categoria = await _context.CategoriasIngreso.AsNoTracking().FirstOrDefaultAsync(c => c.CategoriaIngresoId == id, ct);

            if (categoria == null)
                return NotFound();

            if (!allowAdminAll && categoria.UsuarioId != userId)
                return Forbid();

            return Ok(new CategoriaIngresoDto
            {
                CategoriaIngresoId = categoria.CategoriaIngresoId,
                Nombre = categoria.Nombre,
                UsuarioId = categoria.UsuarioId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoriaIngresoCreateDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var nueva = new CategoriaIngreso
            {
                Nombre = dto.Nombre,
                UsuarioId = userId
            };

            _context.CategoriasIngreso.Add(nueva);
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(Get), new { id = nueva.CategoriaIngresoId }, new CategoriaIngresoDto
            {
                CategoriaIngresoId = nueva.CategoriaIngresoId,
                Nombre = nueva.Nombre,
                UsuarioId = nueva.UsuarioId
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoriaIngresoCreateDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var categoria = await _context.CategoriasIngreso.FirstOrDefaultAsync(c => c.CategoriaIngresoId == id, ct);
            if (categoria == null)
                return NotFound();

            if (categoria.UsuarioId != userId)
                return Forbid();

            categoria.Nombre = dto.Nombre;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var userId = GetUsuarioId();
            var categoria = await _context.CategoriasIngreso.FirstOrDefaultAsync(c => c.CategoriaIngresoId == id, ct);
            if (categoria == null)
                return NotFound();

            if (categoria.UsuarioId != userId)
                return Forbid();

            _context.CategoriasIngreso.Remove(categoria);
            await _context.SaveChangesAsync(ct);

            return NoContent();
        }

        private int GetUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException();
            if (!int.TryParse(userIdClaim, out var userId)) throw new UnauthorizedAccessException();
            return userId;
        }

        private bool EsAdmin() =>
            string.Equals(User.FindFirstValue(ClaimTypes.Role), "Administrador", StringComparison.OrdinalIgnoreCase);
    }
}
