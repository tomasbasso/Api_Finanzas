using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.Models
{
    public class CategoriaGasto
    {
        [Key]
        public int CategoriaGastoId { get; set; }

    public int? UsuarioId { get; set; } = null;
    public string Nombre { get; set; } = string.Empty;
    public Usuario Usuario { get; set; } = null!;
        public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    }

}
