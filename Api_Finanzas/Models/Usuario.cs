using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime FechaRegistro { get; set; }=DateTime.UtcNow;
    public string Rol { get; set; } = string.Empty;
    public ICollection<CuentaBancaria> Cuentas { get; set; } = new List<CuentaBancaria>();
    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    public ICollection<MetaAhorro> MetasAhorro { get; set; } = new List<MetaAhorro>();
    public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    }

}
