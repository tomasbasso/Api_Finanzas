using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.Models
{
    public class CategoriaIngreso
    {
        [Key]
        public int CategoriaIngresoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int? UsuarioId { get; set; } = null;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    }

}
