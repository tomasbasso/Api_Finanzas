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
        public async Task<IActionResult> CrearMeta([FromBody] MetaAhorroDto dto)
        {
            var meta = new MetaAhorro
            {
                UsuarioId = dto.UsuarioId,
                Nombre = dto.Nombre,
                MontoObjetivo = dto.MontoObjetivo,
                FechaLimite = dto.FechaLimite,
                ProgresoActual = dto.ProgresoActual
            };

            _context.MetasAhorro.Add(meta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeta), new { id = meta.MetaId }, meta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMeta(int id, [FromBody] MetaAhorroDto dto)
        {
            var meta = await _context.MetasAhorro.FindAsync(id);
            if (meta == null)
                return NotFound();

            meta.Nombre = dto.Nombre;
            meta.MontoObjetivo = dto.MontoObjetivo;
            meta.FechaLimite = dto.FechaLimite;
            meta.ProgresoActual = dto.ProgresoActual;
            meta.UsuarioId = dto.UsuarioId;

            await _context.SaveChangesAsync();

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
