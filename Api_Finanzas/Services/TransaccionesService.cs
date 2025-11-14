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

        // Validación de categoría según tipo (si viene)
        if (dto.CategoriaId.HasValue)
        {
            var catId = dto.CategoriaId.Value;
            // Verificar que la categoria exista y pertenezca al usuario (ownership)
            var existe = dto.Tipo == "Gasto"
                ? await _db.CategoriasGasto.AnyAsync(c => c.CategoriaGastoId == catId && c.UsuarioId == usuarioId, ct)
                : await _db.CategoriasIngreso.AnyAsync(c => c.CategoriaIngresoId == catId && c.UsuarioId == usuarioId, ct);

            if (!existe) throw new DomainException($"La categoría de {dto.Tipo.ToLower()} no existe o no pertenece al usuario.");
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

        // Usar transacción DB para asegurar consistencia saldo + inserción
        if (_db.Database.IsRelational())
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                _db.Transacciones.Add(entity);
                // Ajustar saldo de la cuenta
                var cuenta = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == dto.CuentaId && c.UsuarioId == usuarioId, ct)
                            ?? throw new DomainException("Cuenta no encontrada o no pertenece al usuario.");

                if (dto.Tipo == "Gasto") cuenta.SaldoActual -= dto.Monto;
                else if (dto.Tipo == "Ingreso") cuenta.SaldoActual += dto.Monto;

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
        else
        {
            _db.Transacciones.Add(entity);
            var cuenta = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == dto.CuentaId && c.UsuarioId == usuarioId, ct)
                        ?? throw new DomainException("Cuenta no encontrada o no pertenece al usuario.");
            if (dto.Tipo == "Gasto") cuenta.SaldoActual -= dto.Monto;
            else if (dto.Tipo == "Ingreso") cuenta.SaldoActual += dto.Monto;
            await _db.SaveChangesAsync(ct);
            return entity.TransaccionId;
        }
    }

    public async Task<TransaccionDto?> ObtenerAsync(int usuarioId, int id, CancellationToken ct)
    {
        return await _db.Transacciones
            .AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId && t.TransaccionId == id)
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

    public async Task<IReadOnlyList<TransaccionDto>> ListarAsync(int usuarioId, DateTime? from, DateTime? to, int page, int size, CancellationToken ct)
    {
        var q = _db.Transacciones.AsNoTracking().Where(t => t.UsuarioId == usuarioId);

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
            ?? throw new NotFoundException($"Transacción {id} no encontrada");

        if (dto.Tipo != "Gasto" && dto.Tipo != "Ingreso")
            throw new DomainException("Tipo debe ser 'Gasto' o 'Ingreso'.");

        if (dto.CategoriaId.HasValue)
        {
            var catId = dto.CategoriaId.Value;
            // Igual que en Crear: validar que la categoria exista y pertenezca al usuario
            var existe = dto.Tipo == "Gasto"
                ? await _db.CategoriasGasto.AnyAsync(c => c.CategoriaGastoId == catId && c.UsuarioId == usuarioId, ct)
                : await _db.CategoriasIngreso.AnyAsync(c => c.CategoriaIngresoId == catId && c.UsuarioId == usuarioId, ct);
            if (!existe) throw new DomainException($"La categoría de {dto.Tipo.ToLower()} no existe o no pertenece al usuario.");
        }

        // Ajustar saldo: revertir impacto previo y aplicar nuevo impacto
        if (_db.Database.IsRelational())
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // Revertir efecto anterior
                var cuentaPrev = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == entity.CuentaId && c.UsuarioId == usuarioId, ct)
                                ?? throw new DomainException("Cuenta anterior no encontrada o no pertenece al usuario.");

                if (entity.Tipo == "Gasto") cuentaPrev.SaldoActual += entity.Monto;
                else if (entity.Tipo == "Ingreso") cuentaPrev.SaldoActual -= entity.Monto;

                // Aplicar nuevos valores
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

                // Ajustar saldo de la (posible) cuenta nueva
                var cuentaNueva = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == dto.CuentaId && c.UsuarioId == usuarioId, ct)
                                ?? throw new DomainException("Cuenta nueva no encontrada o no pertenece al usuario.");

                if (dto.Tipo == "Gasto") cuentaNueva.SaldoActual -= dto.Monto;
                else if (dto.Tipo == "Ingreso") cuentaNueva.SaldoActual += dto.Monto;

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
            // Revertir efecto anterior
            var cuentaPrev = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == entity.CuentaId && c.UsuarioId == usuarioId, ct)
                            ?? throw new DomainException("Cuenta anterior no encontrada o no pertenece al usuario.");

            if (entity.Tipo == "Gasto") cuentaPrev.SaldoActual += entity.Monto;
            else if (entity.Tipo == "Ingreso") cuentaPrev.SaldoActual -= entity.Monto;

            // Aplicar nuevos valores
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
            else if (dto.Tipo == "Ingreso") cuentaNueva.SaldoActual += dto.Monto;

            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteAsync(int usuarioId, int id, CancellationToken ct)
    {
        var entity = await _db.Transacciones
            .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.TransaccionId == id, ct)
            ?? throw new NotFoundException($"Transacción {id} no encontrada");

        if (_db.Database.IsRelational())
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var cuenta = await _db.CuentasBancarias.FirstOrDefaultAsync(c => c.CuentaId == entity.CuentaId && c.UsuarioId == usuarioId, ct)
                            ?? throw new DomainException("Cuenta no encontrada o no pertenece al usuario.");

                // Revertir efecto
                if (entity.Tipo == "Gasto") cuenta.SaldoActual += entity.Monto;
                else if (entity.Tipo == "Ingreso") cuenta.SaldoActual -= entity.Monto;

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

            // Revertir efecto
            if (entity.Tipo == "Gasto") cuenta.SaldoActual += entity.Monto;
            else if (entity.Tipo == "Ingreso") cuenta.SaldoActual -= entity.Monto;

            _db.Transacciones.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task RecalcularSaldosAsync(CancellationToken ct)
    {
        // Calcular saldos para todas las cuentas basándose en SaldoInicial + ingresos - gastos
        if (_db.Database.IsRelational())
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var cuentas = await _db.CuentasBancarias.ToListAsync(ct);

                // Agrupar sumas por cuenta y tipo
                var agregados = await _db.Transacciones
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
            var cuentas = await _db.CuentasBancarias.ToListAsync(ct);

            var agregados = await _db.Transacciones
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
