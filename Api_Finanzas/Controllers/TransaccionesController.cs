using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api_Finanzas.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransaccionesController : ControllerBase
{
    private readonly ITransaccionesService _service;
    public TransaccionesController(ITransaccionesService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] TransaccionCrearDto dto, CancellationToken ct)
    {
        var usuarioId = GetUsuarioId();
        var id = await _service.CrearAsync(usuarioId, dto, ct);
        return CreatedAtAction(nameof(GetTransaccion), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar([FromRoute] int id, [FromBody] TransaccionEditarDto dto, CancellationToken ct)
    {
        var usuarioId = GetUsuarioId();
        await _service.UpdateAsync(usuarioId, id, dto, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar([FromRoute] int id, CancellationToken ct)
    {
        var usuarioId = GetUsuarioId();
        await _service.DeleteAsync(usuarioId, id, ct);
        return NoContent();
    }

    [HttpGet("{id:int}", Name = "GetTransaccion")]
    [ProducesResponseType(typeof(TransaccionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransaccion([FromRoute] int id, CancellationToken ct)
    {
        var usuarioId = GetUsuarioId();
        var item = await _service.ObtenerAsync(usuarioId, id, ct);
        if (item is null) throw new NotFoundException($"Transacción {id} no encontrada");
        return Ok(item);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] DateTime? from, [FromQuery] DateTime? to,
                                            [FromQuery] int page = 0, [FromQuery] int size = 20,
                                            CancellationToken ct = default)
    {
        var usuarioId = GetUsuarioId();
        page = Math.Max(0, page);
        size = Math.Clamp(size, 1, 100);
        var items = await _service.ListarAsync(usuarioId, from, to, page, size, ct);
        Response.Headers["X-Page"] = page.ToString();
        Response.Headers["X-Size"] = size.ToString();
        return Ok(items);
    }

    private int GetUsuarioId()
    {
        var raw = User.FindFirst("sub")?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? throw new UnauthorizedAccessException();
        if (!int.TryParse(raw, out var id)) throw new UnauthorizedAccessException();
        return id;
    }
}
