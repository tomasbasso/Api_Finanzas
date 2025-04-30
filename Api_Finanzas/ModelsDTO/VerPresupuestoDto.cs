namespace Api_Finanzas.ModelsDTO
{
    public class VerPresupuestoDto
    {
        public int PresupuestoId { get; set; }
        public int CategoriaGastoId { get; set; }
        public string NombreCategoria { get; set; }
        public decimal MontoLimite { get; set; }
        public int Mes { get; set; }
        public int Año { get; set; }
    }
}
