namespace Api_Finanzas.ModelsDTO
{
    public class CrearCuentaDto
    {
        public string Nombre { get; set; } = null!;
        public string Banco { get; set; } = null!;
        public string TipoCuenta { get; set; } = null!;
        public decimal SaldoInicial { get; set; }
    }
}
