using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.ModelsDTO
{
    public class EditarPresupuestoDto
    {
        [Required]
        public int CategoriaGastoId { get; set; }

        [Required]
        public decimal MontoLimite { get; set; }

        [Required]
        public int Mes { get; set; }

        [Required]
        public int Anio { get; set; }
    }
}
