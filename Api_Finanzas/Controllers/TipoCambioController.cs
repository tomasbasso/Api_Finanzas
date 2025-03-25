using Api_Finanzas.Models;
using Api_Finanzas.Properties;
using Api_Finanzas.ModelsDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api_Finanzas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoCambioController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public TipoCambioController(FinanzasDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoCambioDto>>> GetAll()
        {
            var tipos = await _context.TiposCambio
                .Select(tc => new TipoCambioDto
                {
                    TipoCambioId = tc.TipoCambioId,
                    MonedaOrigen = tc.MonedaOrigen,
                    MonedaDestino = tc.MonedaDestino,
                    Tasa = tc.Tasa,
                    Fecha = tc.Fecha
                })
                .ToListAsync();

            return Ok(tipos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoCambioDto>> GetById(int id)
        {
            var tc = await _context.TiposCambio.FindAsync(id);
            if (tc == null) return NotFound();

            return Ok(new TipoCambioDto
            {
                TipoCambioId = tc.TipoCambioId,
                MonedaOrigen = tc.MonedaOrigen,
                MonedaDestino = tc.MonedaDestino,
                Tasa = tc.Tasa,
                Fecha = tc.Fecha
            });
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CrearTipoCambioDto dto)
        {
            var nuevo = new TipoCambio
            {
                MonedaOrigen = dto.MonedaOrigen,
                MonedaDestino = dto.MonedaDestino,
                Tasa = dto.Tasa,
                Fecha = dto.Fecha
            };

            _context.TiposCambio.Add(nuevo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = nuevo.TipoCambioId }, nuevo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, CrearTipoCambioDto dto)
        {
            var tipo = await _context.TiposCambio.FindAsync(id);
            if (tipo == null) return NotFound();

            tipo.MonedaOrigen = dto.MonedaOrigen;
            tipo.MonedaDestino = dto.MonedaDestino;
            tipo.Tasa = dto.Tasa;
            tipo.Fecha = dto.Fecha;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var tipo = await _context.TiposCambio.FindAsync(id);
            if (tipo == null) return NotFound();

            _context.TiposCambio.Remove(tipo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
