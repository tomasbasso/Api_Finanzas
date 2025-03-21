namespace Api_Finanzas.Models
{
    public class CategoriaIngreso
    {
        public int CategoriaIngresoId { get; set; }
        public string Nombre { get; set; }
        public int? UsuarioId { get; set; }

        public Usuario Usuario { get; set; }
        public ICollection<Transaccion> Transacciones { get; set; }
    }

}
