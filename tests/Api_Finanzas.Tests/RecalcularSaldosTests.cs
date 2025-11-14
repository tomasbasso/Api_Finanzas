using Api_Finanzas.Models;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas.Properties;
using Api_Finanzas.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api_Finanzas.Tests
{
    public class RecalcularSaldosTests
    {
        private FinanzasDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FinanzasDbContext>()
                .UseInMemoryDatabase(databaseName: "TestRecalcular")
                .Options;
            return new FinanzasDbContext(options);
        }

        [Fact]
        public async Task RecalcularSaldos_CalculaCorrectamente()
        {
            using var db = CreateContext();
            // limpiar DB in-memory
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var cuenta = new CuentaBancaria { CuentaId = 1, UsuarioId = 1, Nombre = "Caja", Banco = "Banco", SaldoInicial = 100m };
            db.CuentasBancarias.Add(cuenta);

            db.Transacciones.Add(new Transaccion { TransaccionId = 1, CuentaId = 1, UsuarioId = 1, Tipo = "Ingreso", Monto = 50m, Fecha = System.DateTime.UtcNow });
            db.Transacciones.Add(new Transaccion { TransaccionId = 2, CuentaId = 1, UsuarioId = 1, Tipo = "Gasto", Monto = 20m, Fecha = System.DateTime.UtcNow });

            await db.SaveChangesAsync();

            var service = new TransaccionesService(db);
            await service.RecalcularSaldosAsync(System.Threading.CancellationToken.None);

            var cuentaActual = await db.CuentasBancarias.FindAsync(1);
            Assert.NotNull(cuentaActual);
            Assert.Equal(130m, cuentaActual.SaldoActual);
        }
    }
}
