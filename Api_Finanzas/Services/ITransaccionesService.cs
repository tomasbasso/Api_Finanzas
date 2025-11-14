// Services/ITransaccionesService.cs
using Api_Finanzas.ModelsDTO;

namespace Api_Finanzas.Services
{
    public interface ITransaccionesService
    {
        Task<int> CrearAsync(int usuarioId, TransaccionCrearDto dto, CancellationToken ct);
        Task<TransaccionDto?> ObtenerAsync(int usuarioId, int id, CancellationToken ct);
        Task<IReadOnlyList<TransaccionDto>> ListarAsync(int usuarioId, DateTime? from, DateTime? to, int page, int size, CancellationToken ct);

        Task UpdateAsync(int usuarioId, int id, TransaccionEditarDto dto, CancellationToken ct);
        Task DeleteAsync(int usuarioId, int id, CancellationToken ct);
        Task RecalcularSaldosAsync(CancellationToken ct);
    }
}
