using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.Models
{
    public class Presupuesto
    {
        [Key]
        public int PresupuestoId { get; set; }
        public int UsuarioId { get; set; }
        public int CategoriaGastoId { get; set; }
        public decimal MontoLimite { get; set; }
        public int Mes { get; set; }
        public int Año { get; set; }

        public Usuario Usuario { get; set; }
        public CategoriaGasto CategoriaGasto { get; set; }
    }

}
