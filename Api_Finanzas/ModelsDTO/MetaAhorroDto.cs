namespace Api_Finanzas.ModelsDTO
{
    public class MetaAhorroDto
    {
        public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
        public decimal MontoObjetivo { get; set; }
        public DateTime FechaLimite { get; set; }
        public decimal ProgresoActual { get; set; }
    }
}
