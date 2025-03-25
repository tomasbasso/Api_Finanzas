namespace Api_Finanzas.ModelsDTO
{
    public class TransaccionCrearDto
    {
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = null!;
        public string? Descripcion { get; set; }
        public int CuentaId { get; set; }
        public int? CategoriaId { get; set; }
        public int UsuarioId { get; set; }
    }
}
