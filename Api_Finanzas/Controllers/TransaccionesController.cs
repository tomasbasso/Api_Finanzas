using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_Finanzas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransaccionesController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public TransaccionesController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransacciones()
        {
            var transacciones = await _context.Transacciones
                .Select(t => new TransaccionDto
                {
                    TransaccionId = t.TransaccionId,
                    Monto = t.Monto,
                    Fecha = t.Fecha,
                    Tipo = t.Tipo,
                    Descripcion = t.Descripcion,
                    CuentaId = t.CuentaId,
                    CategoriaId = t.CategoriaGastoId ?? 0,
                    UsuarioId = t.UsuarioId
                })
                .ToListAsync();

            return Ok(transacciones);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaccion(int id)
        {
            var t = await _context.Transacciones.FindAsync(id);
            if (t == null) return NotFound();

            return Ok(new TransaccionDto
            {
                TransaccionId = t.TransaccionId,
                Monto = t.Monto,
                Fecha = t.Fecha,
                Tipo = t.Tipo,
                Descripcion = t.Descripcion,
                CuentaId = t.CuentaId,
                CategoriaId = t.CategoriaGastoId ?? 0,
                UsuarioId = t.UsuarioId
            });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCrearDto dto)
        {
            var transaccion = new Transaccion
            {
                Monto = dto.Monto,
                Fecha = dto.Fecha,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                CuentaId = dto.CuentaId,
                CategoriaGastoId = dto.CategoriaId,
                UsuarioId = dto.UsuarioId
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransaccion), new { id = transaccion.TransaccionId }, transaccion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, TransaccionCrearDto dto)
        {
            var t = await _context.Transacciones.FindAsync(id);
            if (t == null) return NotFound();

            t.Monto = dto.Monto;
            t.Fecha = dto.Fecha;
            t.Tipo = dto.Tipo;
            t.Descripcion = dto.Descripcion;
            t.CuentaId = dto.CuentaId;
            t.CategoriaGastoId = dto.CategoriaId;
            t.UsuarioId = dto.UsuarioId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var t = await _context.Transacciones.FindAsync(id);
            if (t == null) return NotFound();

            _context.Transacciones.Remove(t);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
