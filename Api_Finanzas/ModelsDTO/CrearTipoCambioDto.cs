namespace Api_Finanzas.ModelsDTO
{
    public class CrearTipoCambioDto
    {
        public string MonedaOrigen { get; set; }
        public string MonedaDestino { get; set; }
        public decimal Tasa { get; set; }
        public DateTime Fecha { get; set; }
    }
}
