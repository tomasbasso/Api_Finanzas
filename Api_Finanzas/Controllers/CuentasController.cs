using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Api_Finanzas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api_Finanzas.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : ControllerBase
    {
        private readonly FinanzasDbContext _context;
        private readonly ITransaccionesService _transService;

        public CuentasController(FinanzasDbContext context, ITransaccionesService transService)
        {
            _context = context;
            _transService = transService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCuentas([FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();

            await _transService.RecalcularSaldosAsync(userId, allowAdminAll, ct);

            var query = _context.CuentasBancarias.AsNoTracking();
            if (!allowAdminAll)
            {
                query = query.Where(c => c.UsuarioId == userId);
            }

            var cuentas = await query
                .Select(c => new CuentaBancariaDto
                {
                    CuentaId = c.CuentaId,
                    Nombre = c.Nombre,
                    Banco = c.Banco,
                    TipoCuenta = c.TipoCuenta,
                    SaldoInicial = c.SaldoInicial,
                    SaldoActual = c.SaldoActual,
                    Saldo = c.SaldoActual,
                    UsuarioId = c.UsuarioId
                })
                .ToListAsync(ct);

            return Ok(cuentas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCuenta(int id, [FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();

            var cuenta = await _context.CuentasBancarias.AsNoTracking().FirstOrDefaultAsync(c => c.CuentaId == id, ct);
            if (cuenta == null) return NotFound();
            if (!allowAdminAll && cuenta.UsuarioId != userId) return Forbid();

            return Ok(new CuentaBancariaDto
            {
                CuentaId = cuenta.CuentaId,
                Nombre = cuenta.Nombre,
                Banco = cuenta.Banco,
                TipoCuenta = cuenta.TipoCuenta,
                SaldoInicial = cuenta.SaldoInicial,
                SaldoActual = cuenta.SaldoActual,
                Saldo = cuenta.SaldoActual,
                UsuarioId = cuenta.UsuarioId
            });
        }

        [HttpPost]
        public async Task<IActionResult> CrearCuenta([FromBody] CrearCuentaDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var cuenta = new CuentaBancaria
            {
                Nombre = dto.Nombre,
                Banco = dto.Banco,
                SaldoInicial = dto.SaldoInicial,
                SaldoActual = dto.SaldoInicial,
                TipoCuenta = dto.TipoCuenta,
                UsuarioId = userId
            };

            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync(ct);

            var result = new CuentaBancariaDto
            {
                CuentaId = cuenta.CuentaId,
                Nombre = cuenta.Nombre,
                Banco = cuenta.Banco,
                TipoCuenta = cuenta.TipoCuenta,
                SaldoInicial = cuenta.SaldoInicial,
                SaldoActual = cuenta.SaldoActual,
                Saldo = cuenta.SaldoActual,
                UsuarioId = cuenta.UsuarioId
            };

            return CreatedAtAction(nameof(GetCuenta), new { id = cuenta.CuentaId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarCuenta(int id, [FromBody] CrearCuentaDto dto, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var cuenta = await _context.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == id, ct);
            if (cuenta == null) return NotFound();
            if (cuenta.UsuarioId != userId) return Forbid();

            cuenta.Nombre = dto.Nombre;
            cuenta.Banco = dto.Banco;
            cuenta.TipoCuenta = dto.TipoCuenta;
            cuenta.SaldoInicial = dto.SaldoInicial;

            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCuenta(int id, CancellationToken ct)
        {
            var userId = GetUsuarioId();

            var cuenta = await _context.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == id, ct);
            if (cuenta == null) return NotFound();
            if (cuenta.UsuarioId != userId) return Forbid();

            _context.CuentasBancarias.Remove(cuenta);
            await _context.SaveChangesAsync(ct);
            return NoContent();
        }

        [HttpPost("recalcular")]
        public async Task<IActionResult> RecalcularSaldos([FromQuery] bool includeAllForAdmin = false, CancellationToken ct = default)
        {
            var userId = GetUsuarioId();
            var allowAdminAll = includeAllForAdmin && EsAdmin();
            await _transService.RecalcularSaldosAsync(userId, allowAdminAll, ct);
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
