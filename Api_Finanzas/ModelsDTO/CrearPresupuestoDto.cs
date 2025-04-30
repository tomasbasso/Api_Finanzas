using System.ComponentModel.DataAnnotations;
namespace Api_Finanzas.ModelsDTO

{
    public class CrearPresupuestoDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int CategoriaGastoId { get; set; }

        [Required]
        public decimal MontoLimite { get; set; }

        [Required]
        public int Mes { get; set; }

        [Required]
        public int Año { get; set; }
    }
}
