using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.Models
{
    public class MetaAhorro
    {
        [Key]
        public int MetaId { get; set; }
        public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
        public decimal MontoObjetivo { get; set; }
        public DateTime FechaLimite { get; set; }
        public decimal ProgresoActual { get; set; }

    public Usuario Usuario { get; set; } = null!;
    }

}
