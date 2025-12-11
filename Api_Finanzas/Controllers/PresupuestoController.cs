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
    public class PresupuestosController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public PresupuestosController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPresupuestos([FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();

            var lista = await _context.Presupuestos
                .Include(p => p.CategoriaGasto)
                .AsNoTracking()
                .Where(p => allowAdminAll || p.UsuarioId == userId)
                .Select(p => new VerPresupuestoDto
                {
                    PresupuestoId = p.PresupuestoId,
                    CategoriaGastoId = p.CategoriaGastoId,
                    NombreCategoria = p.CategoriaGasto.Nombre,
                    MontoLimite = p.MontoLimite,
                    Mes = p.Mes,
                    Anio = p.Anio
                })
                .ToListAsync(ct);

            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VerPresupuestoDto>> GetPresupuesto(int id, [FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();

            var dto = await _context.Presupuestos
                .Include(p => p.CategoriaGasto)
                .AsNoTracking()
                .Where(p => p.PresupuestoId == id)
                .Select(p => new
                {
                    Presupuesto = p,
                    Dto = new VerPresupuestoDto
                    {
                        PresupuestoId = p.PresupuestoId,
                        CategoriaGastoId = p.CategoriaGastoId,
                        NombreCategoria = p.CategoriaGasto.Nombre,
                        MontoLimite = p.MontoLimite,
                        Mes = p.Mes,
                        Anio = p.Anio
                    }
                })
                .FirstOrDefaultAsync(ct);

            if (dto == null)
                return NotFound();

            if (!allowAdminAll && dto.Presupuesto.UsuarioId != userId)
                return Forbid();

            return Ok(dto.Dto);
        }

        [HttpPost]
        public async Task<IActionResult> CrearPresupuesto([FromBody] CrearPresupuestoDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var categoriaEsDelUsuario = await _context.CategoriasGasto.AnyAsync(c => c.CategoriaGastoId == dto.CategoriaGastoId && c.UsuarioId == userId, ct);
            if (!categoriaEsDelUsuario)
                return Forbid();

            var entidad = new Presupuesto
            {
                UsuarioId = userId,
                CategoriaGastoId = dto.CategoriaGastoId,
                MontoLimite = dto.MontoLimite,
                Mes = dto.Mes,
                Anio = dto.Anio
            };

            _context.Presupuestos.Add(entidad);
            await _context.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetPresupuesto), new { id = entidad.PresupuestoId }, new { id = entidad.PresupuestoId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarPresupuesto(int id, [FromBody] EditarPresupuestoDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var presupuesto = await _context.Presupuestos.FirstOrDefaultAsync(p => p.PresupuestoId == id, ct);
            if (presupuesto == null) return NotFound();
            if (presupuesto.UsuarioId != userId) return Forbid();

            var categoriaEsDelUsuario = await _context.CategoriasGasto.AnyAsync(c => c.CategoriaGastoId == dto.CategoriaGastoId && c.UsuarioId == userId, ct);
            if (!categoriaEsDelUsuario)
                return Forbid();

            presupuesto.CategoriaGastoId = dto.CategoriaGastoId;
            presupuesto.MontoLimite = dto.MontoLimite;
            presupuesto.Mes = dto.Mes;
            presupuesto.Anio = dto.Anio;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPresupuesto(int id, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var presupuesto = await _context.Presupuestos.FirstOrDefaultAsync(p => p.PresupuestoId == id, ct);
            if (presupuesto == null) return NotFound();
            if (presupuesto.UsuarioId != userId) return Forbid();

            _context.Presupuestos.Remove(presupuesto);
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
