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
            var lista = await _context.Presupuestos
                .Include(p => p.CategoriaGasto)
                .Select(p => new VerPresupuestoDto
                {
                    PresupuestoId = p.PresupuestoId,
                    CategoriaGastoId = p.CategoriaGastoId,
                    NombreCategoria = p.CategoriaGasto.Nombre,
                    MontoLimite = p.MontoLimite,
                    Mes = p.Mes,
                    Año = p.Año
                })
                .ToListAsync();

            return Ok(lista);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<VerPresupuestoDto>> GetPresupuesto(int id)
        {
            var dto = await _context.Presupuestos
                .Include(p => p.CategoriaGasto)
                .Where(p => p.PresupuestoId == id)
                .Select(p => new VerPresupuestoDto
                {
                    PresupuestoId = p.PresupuestoId,
                    CategoriaGastoId = p.CategoriaGastoId,
                    NombreCategoria = p.CategoriaGasto.Nombre,
                    MontoLimite = p.MontoLimite,
                    Mes = p.Mes,
                    Año = p.Año
                })
                .FirstOrDefaultAsync();

            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CrearPresupuesto([FromBody] CrearPresupuestoDto dto)
        {
            var entidad = new Presupuesto
            {
                UsuarioId = dto.UsuarioId,
                CategoriaGastoId = dto.CategoriaGastoId,
                MontoLimite = dto.MontoLimite,
                Mes = dto.Mes,
                Año = dto.Año
            };
            _context.Presupuestos.Add(entidad);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPresupuesto), new { id = entidad.PresupuestoId }, entidad);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarPresupuesto(int id, [FromBody] EditarPresupuestoDto dto)
        {
            var p = await _context.Presupuestos.FindAsync(id);
            if (p == null) return NotFound();

            p.UsuarioId = dto.UsuarioId;
            p.CategoriaGastoId = dto.CategoriaGastoId;
            p.MontoLimite = dto.MontoLimite;
            p.Mes = dto.Mes;
            p.Año = dto.Año;

            await _context.SaveChangesAsync();
            return Ok(p);
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
