namespace Api_Finanzas.Models
{
    public class TipoCambio
    {
        public int TipoCambioId { get; set; }
        public string MonedaOrigen { get; set; }
        public string MonedaDestino { get; set; }
        public decimal Tasa { get; set; }
        public DateTime Fecha { get; set; }
    }

}
