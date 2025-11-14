using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.Models
{
    public class Transaccion
    {
        [Key]
        public int TransaccionId { get; set; }
        public int UsuarioId { get; set; }
        public int CuentaId { get; set; }
        public int? CategoriaGastoId { get; set; }
        public int? CategoriaIngresoId { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty; // "Gasto" o "Ingreso"
    public string Descripcion { get; set; } = string.Empty;

    public Usuario Usuario { get; set; } = null!;
    public CuentaBancaria Cuenta { get; set; } = null!;
    public CategoriaGasto CategoriaGasto { get; set; } = null!;
    public CategoriaIngreso CategoriaIngreso { get; set; } = null!;
    }

}
