// Services/TransaccionesService.cs
using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Microsoft.EntityFrameworkCore;

namespace Api_Finanzas.Services;

public sealed class TransaccionesService : ITransaccionesService
{
    private readonly FinanzasDbContext _db;
    public TransaccionesService(FinanzasDbContext db) => _db = db;

    public async Task<int> CrearAsync(int usuarioId, TransaccionCrearDto dto, CancellationToken ct)
    {
        if (dto.Tipo != "Gasto" && dto.Tipo != "Ingreso")
            throw new DomainException("Tipo debe ser 'Gasto' o 'Ingreso'.");

        if (dto.CategoriaId.HasValue)
        {
            var catId = dto.CategoriaId.Value;
            var existe = dto.Tipo == "Gasto"
                ? await _db.CategoriasGasto.AnyAsync(c => c.CategoriaGastoId == catId && c.UsuarioId == usuarioId, ct)
                : await _db.CategoriasIngreso.AnyAsync(c => c.CategoriaIngresoId == catId && c.UsuarioId == usuarioId, ct);

            if (!existe) throw new DomainException($"La categoria de {dto.Tipo.ToLower()} no existe o no pertenece al usuario.");
        }

        var entity = new Transaccion
        {
            UsuarioId = usuarioId,
            CuentaId = dto.CuentaId,
            CategoriaGastoId = dto.Tipo == "Gasto" ? dto.CategoriaId : null,
            CategoriaIngresoId = dto.Tipo == "Ingreso" ? dto.CategoriaId : null,
            Monto = dto.Monto,
            Fecha = dto.Fecha,
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion ?? string.Empty
        };

        if (_db.Database.IsRelational())
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                _db.Transacciones.Add(entity);
                var cuenta = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == dto.CuentaId && c.UsuarioId == usuarioId, ct)
                             ?? throw new DomainException("Cuenta no encontrada o no pertenece al usuario.");

                if (dto.Tipo == "Gasto") cuenta.SaldoActual -= dto.Monto;
                else cuenta.SaldoActual += dto.Monto;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return entity.TransaccionId;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        _db.Transacciones.Add(entity);
        var cuentaNoRel = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == dto.CuentaId && c.UsuarioId == usuarioId, ct)
                        ?? throw new DomainException("Cuenta no encontrada o no pertenece al usuario.");
        if (dto.Tipo == "Gasto") cuentaNoRel.SaldoActual -= dto.Monto;
        else cuentaNoRel.SaldoActual += dto.Monto;
        await _db.SaveChangesAsync(ct);
        return entity.TransaccionId;
    }

    public async Task<TransaccionDto?> ObtenerAsync(int usuarioId, int id, bool allowAdminAll, CancellationToken ct)
    {
        var query = _db.Transacciones.AsNoTracking().Where(t => t.TransaccionId == id);
        if (!allowAdminAll)
        {
            query = query.Where(t => t.UsuarioId == usuarioId);
        }

        return await query
            .Select(t => new TransaccionDto
            {
                TransaccionId = t.TransaccionId,
                Monto = t.Monto,
                Fecha = t.Fecha,
                Tipo = t.Tipo,
                Descripcion = t.Descripcion,
                CuentaId = t.CuentaId,
                CategoriaId = t.CategoriaGastoId ?? t.CategoriaIngresoId,
                UsuarioId = t.UsuarioId
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<TransaccionDto>> ListarAsync(int usuarioId, bool allowAdminAll, DateTime? from, DateTime? to, int page, int size, CancellationToken ct)
    {
        var q = _db.Transacciones.AsNoTracking();
        if (!allowAdminAll)
        {
            q = q.Where(t => t.UsuarioId == usuarioId);
        }

        if (from.HasValue) q = q.Where(t => t.Fecha >= from.Value);
        if (to.HasValue) q = q.Where(t => t.Fecha <= to.Value);

        return await q
            .OrderByDescending(t => t.Fecha)
            .Skip(page * size)
            .Take(size)
            .Select(t => new TransaccionDto
            {
                TransaccionId = t.TransaccionId,
                Monto = t.Monto,
                Fecha = t.Fecha,
                Tipo = t.Tipo,
                Descripcion = t.Descripcion,
                CuentaId = t.CuentaId,
                CategoriaId = t.CategoriaGastoId ?? t.CategoriaIngresoId,
                UsuarioId = t.UsuarioId
            })
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(int usuarioId, int id, TransaccionEditarDto dto, CancellationToken ct)
    {
        var entity = await _db.Transacciones
            .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.TransaccionId == id, ct)
            ?? throw new NotFoundException($"Transaccion {id} no encontrada");

        if (dto.Tipo != "Gasto" && dto.Tipo != "Ingreso")
            throw new DomainException("Tipo debe ser 'Gasto' o 'Ingreso'.");

        if (dto.CategoriaId.HasValue)
        {
            var catId = dto.CategoriaId.Value;
            var existe = dto.Tipo == "Gasto"
                ? await _db.CategoriasGasto.AnyAsync(c => c.CategoriaGastoId == catId && c.UsuarioId == usuarioId, ct)
                : await _db.CategoriasIngreso.AnyAsync(c => c.CategoriaIngresoId == catId && c.UsuarioId == usuarioId, ct);
            if (!existe) throw new DomainException($"La categoria de {dto.Tipo.ToLower()} no existe o no pertenece al usuario.");
        }

        if (_db.Database.IsRelational())
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var cuentaPrev = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == entity.CuentaId && c.UsuarioId == usuarioId, ct)
                                ?? throw new DomainException("Cuenta anterior no encontrada o no pertenece al usuario.");

                if (entity.Tipo == "Gasto") cuentaPrev.SaldoActual += entity.Monto;
                else cuentaPrev.SaldoActual -= entity.Monto;

                entity.Monto = dto.Monto;
                entity.Tipo = dto.Tipo;
                entity.Fecha = dto.Fecha;
                entity.CuentaId = dto.CuentaId;
                entity.Descripcion = dto.Descripcion ?? string.Empty;

                if (dto.Tipo == "Gasto")
                {
                    entity.CategoriaGastoId = dto.CategoriaId;
                    entity.CategoriaIngresoId = null;
                }
                else
                {
                    entity.CategoriaIngresoId = dto.CategoriaId;
                    entity.CategoriaGastoId = null;
                }

                var cuentaNueva = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == dto.CuentaId && c.UsuarioId == usuarioId, ct)
                                ?? throw new DomainException("Cuenta nueva no encontrada o no pertenece al usuario.");

                if (dto.Tipo == "Gasto") cuentaNueva.SaldoActual -= dto.Monto;
                else cuentaNueva.SaldoActual += dto.Monto;

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
        else
        {
            var cuentaPrev = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == entity.CuentaId && c.UsuarioId == usuarioId, ct)
                            ?? throw new DomainException("Cuenta anterior no encontrada o no pertenece al usuario.");

            if (entity.Tipo == "Gasto") cuentaPrev.SaldoActual += entity.Monto;
            else cuentaPrev.SaldoActual -= entity.Monto;

            entity.Monto = dto.Monto;
            entity.Tipo = dto.Tipo;
            entity.Fecha = dto.Fecha;
            entity.CuentaId = dto.CuentaId;
            entity.Descripcion = dto.Descripcion ?? string.Empty;

            if (dto.Tipo == "Gasto")
            {
                entity.CategoriaGastoId = dto.CategoriaId;
                entity.CategoriaIngresoId = null;
            }
            else
            {
                entity.CategoriaIngresoId = dto.CategoriaId;
                entity.CategoriaGastoId = null;
            }

            var cuentaNueva = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == dto.CuentaId && c.UsuarioId == usuarioId, ct)
                            ?? throw new DomainException("Cuenta nueva no encontrada o no pertenece al usuario.");

            if (dto.Tipo == "Gasto") cuentaNueva.SaldoActual -= dto.Monto;
            else cuentaNueva.SaldoActual += dto.Monto;

            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteAsync(int usuarioId, int id, CancellationToken ct)
    {
        var entity = await _db.Transacciones
            .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.TransaccionId == id, ct)
            ?? throw new NotFoundException($"Transaccion {id} no encontrada");

        if (_db.Database.IsRelational())
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var cuenta = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == entity.CuentaId && c.UsuarioId == usuarioId, ct)
                            ?? throw new DomainException("Cuenta no encontrada o no pertenece al usuario.");

                if (entity.Tipo == "Gasto") cuenta.SaldoActual += entity.Monto;
                else cuenta.SaldoActual -= entity.Monto;

                _db.Transacciones.Remove(entity);
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
        else
        {
            var cuenta = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == entity.CuentaId && c.UsuarioId == usuarioId, ct)
                        ?? throw new DomainException("Cuenta no encontrada o no pertenece al usuario.");

            if (entity.Tipo == "Gasto") cuenta.SaldoActual += entity.Monto;
            else cuenta.SaldoActual -= entity.Monto;

            _db.Transacciones.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RecalcularSaldosAsync(int usuarioId, bool allowAdminAll, CancellationToken ct)
    {
        var cuentasQuery = _db.CuentasBancarias.AsQueryable();
        var transQuery = _db.Transacciones.AsQueryable();

        if (!allowAdminAll)
        {
            cuentasQuery = cuentasQuery.Where(c => c.UsuarioId == usuarioId);
            transQuery = transQuery.Where(t => t.UsuarioId == usuarioId);
        }

        if (_db.Database.IsRelational())
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var cuentas = await cuentasQuery.ToListAsync(ct);

                var agregados = await transQuery
                    .GroupBy(t => new { t.CuentaId, t.Tipo })
                    .Select(g => new { g.Key.CuentaId, g.Key.Tipo, Total = g.Sum(x => x.Monto) })
                    .ToListAsync(ct);

                var mapa = agregados
                    .GroupBy(a => a.CuentaId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var cuenta in cuentas)
                {
                    var inicial = cuenta.SaldoInicial;
                    decimal ingresos = 0m;
                    decimal gastos = 0m;
                    if (mapa.TryGetValue(cuenta.CuentaId, out var items))
                    {
                        foreach (var it in items)
                        {
                            if (it.Tipo == "Ingreso") ingresos += it.Total;
                            else if (it.Tipo == "Gasto") gastos += it.Total;
                        }
                    }
                    cuenta.SaldoActual = inicial + ingresos - gastos;
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
        else
        {
            var cuentas = await cuentasQuery.ToListAsync(ct);

            var agregados = await transQuery
                .GroupBy(t => new { t.CuentaId, t.Tipo })
                .Select(g => new { g.Key.CuentaId, g.Key.Tipo, Total = g.Sum(x => x.Monto) })
                .ToListAsync(ct);

            var mapa = agregados
                .GroupBy(a => a.CuentaId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var cuenta in cuentas)
            {
                var inicial = cuenta.SaldoInicial;
                decimal ingresos = 0m;
                decimal gastos = 0m;
                if (mapa.TryGetValue(cuenta.CuentaId, out var items))
                {
                    foreach (var it in items)
                    {
                        if (it.Tipo == "Ingreso") ingresos += it.Total;
                        else if (it.Tipo == "Gasto") gastos += it.Total;
                    }
                }
                cuenta.SaldoActual = inicial + ingresos - gastos;
            }

            await _db.SaveChangesAsync(ct);
        }
    }
}
