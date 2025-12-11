namespace Api_Finanzas.ModelsDTO
{
    public class CuentaBancariaDto
    {
        public int CuentaId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Banco { get; set; } = null!;
        public string TipoCuenta { get; set; } = null!;
        public decimal SaldoInicial { get; set; }
        public decimal SaldoActual { get; set; }
        // Alias para UI: asegura que se pueda mostrar el saldo correcto aunque el cliente espere "Saldo".
        public decimal Saldo { get; set; }
        public int UsuarioId { get; set; }
    }
}
