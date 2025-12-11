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
    public class MetasAhorroController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public MetasAhorroController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMetas([FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();

            var metas = await _context.MetasAhorro
                .AsNoTracking()
                .Where(m => allowAdminAll || m.UsuarioId == userId)
                .ToListAsync(ct);

            return Ok(metas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeta(int id, [FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();

            var meta = await _context.MetasAhorro.AsNoTracking().FirstOrDefaultAsync(m => m.MetaId == id, ct);
            if (meta == null)
                return NotFound();

            if (!allowAdminAll && meta.UsuarioId != userId)
                return Forbid();

            return Ok(meta);
        }

        [HttpPost]
        public async Task<IActionResult> CrearMeta([FromBody] MetaAhorroDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var meta = new MetaAhorro
            {
                UsuarioId = userId,
                Nombre = dto.Nombre,
                MontoObjetivo = dto.MontoObjetivo,
                FechaLimite = dto.FechaLimite,
                ProgresoActual = dto.ProgresoActual
            };

            _context.MetasAhorro.Add(meta);
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetMeta), new { id = meta.MetaId }, new { id = meta.MetaId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMeta(int id, [FromBody] MetaAhorroDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();
            var meta = await _context.MetasAhorro.FirstOrDefaultAsync(m => m.MetaId == id, ct);
            if (meta == null)
                return NotFound();

            if (meta.UsuarioId != userId)
                return Forbid();

            meta.Nombre = dto.Nombre;
            meta.MontoObjetivo = dto.MontoObjetivo;
            meta.FechaLimite = dto.FechaLimite;
            meta.ProgresoActual = dto.ProgresoActual;

            await _context.SaveChangesAsync(ct);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMeta(int id, CancellationToken ct)
        {
            var userId = GetUsuarioId();
            var meta = await _context.MetasAhorro.FirstOrDefaultAsync(m => m.MetaId == id, ct);
            if (meta == null)
                return NotFound();

            if (meta.UsuarioId != userId)
                return Forbid();

            _context.MetasAhorro.Remove(meta);
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
