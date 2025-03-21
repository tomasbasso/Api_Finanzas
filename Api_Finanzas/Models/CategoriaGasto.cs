namespace Api_Finanzas.Models
{
    public class CategoriaGasto
    {
        public int CategoriaGastoId { get; set; }
        public string Nombre { get; set; }
        public int? UsuarioId { get; set; }

        public Usuario Usuario { get; set; }
        public ICollection<Transaccion> Transacciones { get; set; }
        public ICollection<Presupuesto> Presupuestos { get; set; }
    }

}
