using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_Finanzas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CuentasController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public CuentasController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCuentas()
        {
            var cuentas = await _context.CuentasBancarias
                .Select(c => new CuentaBancariaDto
                {
                    CuentaId = c.CuentaId,
                    Nombre = c.Nombre,
                    Banco = c.Banco,
                    SaldoInicial = c.SaldoInicial,
                    UsuarioId = c.UsuarioId
                })
                .ToListAsync();

            return Ok(cuentas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCuenta(int id)
        {
            var cuenta = await _context.CuentasBancarias.FindAsync(id);
            if (cuenta == null) return NotFound();

            return Ok(new CuentaBancariaDto
            {
                CuentaId = cuenta.CuentaId,
                Nombre = cuenta.Nombre,
                Banco = cuenta.Banco,
                SaldoInicial = cuenta.SaldoInicial,
                UsuarioId = cuenta.UsuarioId
            });
        }

        [HttpPost]
        public async Task<IActionResult> CrearCuenta(CrearCuentaDto dto)
        {
            var cuenta = new CuentaBancaria
            {
                Nombre = dto.Nombre,
                SaldoInicial = dto.SaldoInicial,
                UsuarioId = dto.UsuarioId
            };

            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCuenta), new { id = cuenta.CuentaId }, cuenta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarCuenta(int id, CrearCuentaDto dto)
        {
            var cuenta = await _context.CuentasBancarias.FindAsync(id);
            if (cuenta == null) return NotFound();

            cuenta.Nombre = dto.Nombre;
            cuenta.Banco = dto.Banco;
            cuenta.SaldoInicial = dto.SaldoInicial;
            cuenta.UsuarioId = dto.UsuarioId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCuenta(int id)
        {
            var cuenta = await _context.CuentasBancarias.FindAsync(id);
            if (cuenta == null) return NotFound();

            _context.CuentasBancarias.Remove(cuenta);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
