namespace Api_Finanzas.ModelsDTO
{
    public class CrearCuentaDto
    {
        public string Nombre { get; set; } = null!;
        public decimal SaldoInicial { get; set; }
        public int UsuarioId { get; set; }
    }
}
