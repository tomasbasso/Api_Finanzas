using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api_Finanzas.Services;

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
        public async Task<IActionResult> GetCuentas(CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();
            var userId = int.Parse(userIdClaim);

            // Recalcula saldos antes de listar para asegurar valores actualizados.
            await _transService.RecalcularSaldosAsync(ct);

            var cuentas = await _context.CuentasBancarias
                .Where(c => c.UsuarioId == userId)
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
                .ToListAsync();

            return Ok(cuentas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCuenta(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();
            var userId = int.Parse(userIdClaim);
            var cuenta = await _context.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == id && c.UsuarioId == userId);
            if (cuenta == null) return NotFound();

            return Ok(new CuentaBancariaDto
            {
                CuentaId = cuenta.CuentaId,
                Nombre = cuenta.Nombre,
                Banco = cuenta.Banco,
                TipoCuenta= cuenta.TipoCuenta,
                SaldoInicial = cuenta.SaldoInicial,
                SaldoActual = cuenta.SaldoActual,
                Saldo = cuenta.SaldoActual,
                UsuarioId = cuenta.UsuarioId
            });
        }

        [HttpPost]
        public async Task<IActionResult> CrearCuenta(CrearCuentaDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var cuenta = new CuentaBancaria
            {
                Nombre = dto.Nombre,
                Banco = dto.Banco,
                SaldoInicial = dto.SaldoInicial,
                SaldoActual = dto.SaldoInicial,
                TipoCuenta= dto.TipoCuenta,
                UsuarioId = userId
            };

            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCuenta), new { id = cuenta.CuentaId }, cuenta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarCuenta(int id, CrearCuentaDto dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var cuenta = await _context.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == id && c.UsuarioId == userId);
            if (cuenta == null) return NotFound();

            cuenta.Nombre = dto.Nombre;
            cuenta.Banco = dto.Banco;
            cuenta.TipoCuenta = dto.TipoCuenta;
            cuenta.SaldoInicial = dto.SaldoInicial;
            cuenta.UsuarioId = userId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCuenta(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();
            var userId = int.Parse(userIdClaim);

            var cuenta = await _context.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == id && c.UsuarioId == userId);
            if (cuenta == null) return NotFound();

            _context.CuentasBancarias.Remove(cuenta);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("recalcular")]
        public async Task<IActionResult> RecalcularSaldos(CancellationToken ct)
        {
            await _transService.RecalcularSaldosAsync(ct);
            return NoContent();
        }
    }
}
