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
    public class CategoriasGastoController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public CategoriasGastoController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategorias([FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();

            var categorias = await _context.CategoriasGasto
                .AsNoTracking()
                .Where(c => allowAdminAll || c.UsuarioId == userId)
                .Select(c => new CategoriaGastoDto
                {
                    CategoriaGastoId = c.CategoriaGastoId,
                    Nombre = c.Nombre,
                    UsuarioId = c.UsuarioId
                }).ToListAsync(ct);

            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoria(int id, [FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();

            var categoria = await _context.CategoriasGasto.AsNoTracking().FirstOrDefaultAsync(c => c.CategoriaGastoId == id, ct);
            if (categoria == null)
                return NotFound();

            if (!allowAdminAll && categoria.UsuarioId != userId)
                return Forbid();

            var dto = new CategoriaGastoDto
            {
                CategoriaGastoId = categoria.CategoriaGastoId,
                Nombre = categoria.Nombre,
                UsuarioId = categoria.UsuarioId
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaGastoDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var nueva = new CategoriaGasto
            {
                Nombre = dto.Nombre,
                UsuarioId = userId
            };

            _context.CategoriasGasto.Add(nueva);
            await _context.SaveChangesAsync(ct);

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
        public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] CategoriaGastoDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var categoria = await _context.CategoriasGasto.FirstOrDefaultAsync(c => c.CategoriaGastoId == id, ct);
            if (categoria == null)
                return NotFound();

            if (categoria.UsuarioId != userId)
                return Forbid();

            categoria.Nombre = dto.Nombre;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCategoria(int id, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var categoria = await _context.CategoriasGasto.FirstOrDefaultAsync(c => c.CategoriaGastoId == id, ct);
            if (categoria == null)
                return NotFound();

            if (categoria.UsuarioId != userId)
                return Forbid();

            _context.CategoriasGasto.Remove(categoria);
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
