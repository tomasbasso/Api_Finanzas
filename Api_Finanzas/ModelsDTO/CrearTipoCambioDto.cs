namespace Api_Finanzas.ModelsDTO
{
    public class CrearTipoCambioDto
    {
    public string MonedaOrigen { get; set; } = string.Empty;
    public string MonedaDestino { get; set; } = string.Empty;
        public decimal Tasa { get; set; }
        public DateTime Fecha { get; set; }
    }
}
