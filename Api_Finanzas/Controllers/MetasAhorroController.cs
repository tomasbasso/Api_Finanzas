using Api_Finanzas.Models;
using Api_Finanzas.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_Finanzas.Controllers
{
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
        public async Task<IActionResult> GetMetas()
        {
            var metas = await _context.MetasAhorro.ToListAsync();
            return Ok(metas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeta(int id)
        {
            var meta = await _context.MetasAhorro.FindAsync(id);
            if (meta == null)
                return NotFound();

            return Ok(meta);
        }

        [HttpPost]
        public async Task<IActionResult> CrearMeta([FromBody] MetaAhorro meta)
        {
            _context.MetasAhorro.Add(meta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeta), new { id = meta.MetaId }, meta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMeta(int id, [FromBody] MetaAhorro meta)
        {
            if (id != meta.MetaId)
                return BadRequest("El ID no coincide.");

            _context.Entry(meta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.MetasAhorro.Any(m => m.MetaId == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMeta(int id)
        {
            var meta = await _context.MetasAhorro.FindAsync(id);
            if (meta == null)
                return NotFound();

            _context.MetasAhorro.Remove(meta);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
