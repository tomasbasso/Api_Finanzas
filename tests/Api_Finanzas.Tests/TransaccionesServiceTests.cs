using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Api_Finanzas.Properties;
using Api_Finanzas.Services;
using Api_Finanzas.ModelsDTO;
using Api_Finanzas; // para DomainException y NotFoundException

namespace Api_Finanzas.Tests
{
    public class TransaccionesServiceTests
    {
        private FinanzasDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<FinanzasDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new FinanzasDbContext(options);
        }

        [Fact]
        public async Task CrearTransaccion_AjustaSaldo()
        {
            // arrange
            using var context = CreateContext(Guid.NewGuid().ToString());

            var cuenta = new Api_Finanzas.Models.CuentaBancaria
            {
                UsuarioId = 1,
                Nombre = "Caja",
                Banco = "Test",
                SaldoInicial = 100m,
                SaldoActual = 100m,
                TipoCuenta = "Corriente"
            };
            context.CuentasBancarias.Add(cuenta);
            await context.SaveChangesAsync();

            var svc = new TransaccionesService(context);

            var dto = new TransaccionCrearDto
            {
                Monto = 10m,
                Tipo = "Gasto",
                Fecha = DateTime.UtcNow,
                CuentaId = cuenta.CuentaId,
                Descripcion = "Compra"
            };

            // act
            var id = await svc.CrearAsync(1, dto, CancellationToken.None);

            // assert
            var cuentaDb = await context.CuentasBancarias.FindAsync(cuenta.CuentaId);
            Assert.Equal(90m, cuentaDb.SaldoActual);
        }

        [Fact]
        public async Task CrearTransaccion_CategoriaNoPertenece_Throws()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());

            var cuenta = new Api_Finanzas.Models.CuentaBancaria
            {
                UsuarioId = 1,
                Nombre = "Caja",
                Banco = "Test",
                SaldoInicial = 0m,
                SaldoActual = 0m,
                TipoCuenta = "Corriente"
            };
            context.CuentasBancarias.Add(cuenta);
            await context.SaveChangesAsync();

            var svc = new TransaccionesService(context);
            var dto = new TransaccionCrearDto
            {
                Monto = 5m,
                Tipo = "Gasto",
                Fecha = DateTime.UtcNow,
                CuentaId = cuenta.CuentaId,
                CategoriaId = 999 // no existe
            };

            await Assert.ThrowsAsync<Api_Finanzas.DomainException>(async () =>
            {
                await svc.CrearAsync(1, dto, CancellationToken.None);
            });
        }

        [Fact]
        public async Task EditarTransaccion_CambiaCuenta_AjustaSaldos()
        {
            using var context = CreateContext(Guid.NewGuid().ToString());

            var cuenta1 = new Api_Finanzas.Models.CuentaBancaria
            {
                UsuarioId = 1,
                Nombre = "Caja1",
                Banco = "Test",
                SaldoInicial = 100m,
                SaldoActual = 100m,
                TipoCuenta = "Corriente"
            };
            var cuenta2 = new Api_Finanzas.Models.CuentaBancaria
            {
                UsuarioId = 1,
                Nombre = "Caja2",
                Banco = "Test",
                SaldoInicial = 50m,
                SaldoActual = 50m,
                TipoCuenta = "Corriente"
            };
            context.CuentasBancarias.AddRange(cuenta1, cuenta2);
            await context.SaveChangesAsync();

            var trans = new Api_Finanzas.Models.Transaccion
            {
                UsuarioId = 1,
                CuentaId = cuenta1.CuentaId,
                Monto = 20m,
                Tipo = "Gasto",
                Fecha = DateTime.UtcNow,
                Descripcion = "Compra"
            };
            context.Transacciones.Add(trans);
            // Simular saldo ya afectado
            cuenta1.SaldoActual -= 20m;
            await context.SaveChangesAsync();

            var svc = new TransaccionesService(context);

            var editDto = new Api_Finanzas.ModelsDTO.TransaccionEditarDto
            {
                Monto = 20m,
                Tipo = "Gasto",
                Fecha = DateTime.UtcNow,
                CuentaId = cuenta2.CuentaId,
                Descripcion = "Mover"
            };

            await svc.UpdateAsync(1, trans.TransaccionId, editDto, CancellationToken.None);

            var db1 = await context.CuentasBancarias.FindAsync(cuenta1.CuentaId);
            var db2 = await context.CuentasBancarias.FindAsync(cuenta2.CuentaId);
            Assert.Equal(100m, db1.SaldoActual); // revertido
            Assert.Equal(30m, db2.SaldoActual); // 50 - 20
        }
    }
}
