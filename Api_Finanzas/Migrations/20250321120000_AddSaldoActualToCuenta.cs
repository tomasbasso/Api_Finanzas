using Api_Finanzas.Properties;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Api_Finanzas.Properties.Migrations
{
    [DbContext(typeof(FinanzasDbContext))]
    [Migration("20250321120000_AddSaldoActualToCuenta")]
    public partial class AddSaldoActualToCuenta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "saldoactual",
                table: "cuentasbancarias",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(@"
UPDATE cb
SET cb.saldoactual = cb.saldoinicial + ISNULL(ing.total_ingresos, 0) - ISNULL(gas.total_gastos, 0)
FROM cuentasbancarias cb
LEFT JOIN (
    SELECT cuentaid, SUM(monto) AS total_ingresos
    FROM transacciones
    WHERE tipo = 'Ingreso'
    GROUP BY cuentaid
) ing ON ing.cuentaid = cb.cuentaid
LEFT JOIN (
    SELECT cuentaid, SUM(monto) AS total_gastos
    FROM transacciones
    WHERE tipo = 'Gasto'
    GROUP BY cuentaid
) gas ON gas.cuentaid = cb.cuentaid;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "saldoactual",
                table: "cuentasbancarias");
        }
    }
}
