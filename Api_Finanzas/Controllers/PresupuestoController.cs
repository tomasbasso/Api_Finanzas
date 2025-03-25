using Api_Finanzas.Models;
using Api_Finanzas.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_Finanzas.Controllers
{
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
        public async Task<IActionResult> GetPresupuestos()
        {
            var presupuestos = await _context.Presupuestos
                .Select(p => new
                {
                    p.PresupuestoId,
                    p.MontoLimite,
                    p.CategoriaGastoId,
                    p.UsuarioId
                }).ToListAsync();

            return Ok(presupuestos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPresupuesto(int id)
        {
            var presupuesto = await _context.Presupuestos.FindAsync(id);
            if (presupuesto == null) return NotFound();

            return Ok(presupuesto);
        }

        [HttpPost]
        public async Task<IActionResult> CrearPresupuesto(Presupuesto presupuesto)
        {
            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();
            return Ok(presupuesto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarPresupuesto(int id, Presupuesto nuevo)
        {
            var presupuesto = await _context.Presupuestos.FindAsync(id);
            if (presupuesto == null) return NotFound();

            presupuesto.MontoLimite = nuevo.MontoLimite;
            presupuesto.CategoriaGastoId = nuevo.CategoriaGastoId;
            presupuesto.UsuarioId = nuevo.UsuarioId;

            await _context.SaveChangesAsync();
            return Ok(presupuesto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPresupuesto(int id)
        {
            var presupuesto = await _context.Presupuestos.FindAsync(id);
            if (presupuesto == null) return NotFound();

            _context.Presupuestos.Remove(presupuesto);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
