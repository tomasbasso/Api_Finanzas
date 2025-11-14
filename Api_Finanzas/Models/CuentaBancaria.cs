using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_Finanzas.Models
{
    public class CuentaBancaria
    {
        [Key]
        public int CuentaId { get; set; }
        public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
        public decimal SaldoInicial { get; set; }
    // Saldo actual cacheado; se debe mantener consistente al crear/editar/eliminar transacciones.
    // Marcado como [NotMapped] para evitar cambios en el esquema de la base de datos mientras
    // decidimos cuándo y cómo persistir este valor (sin crear migraciones automáticas).
    [NotMapped]
    public decimal SaldoActual { get; set; }
    public string TipoCuenta { get; set; } = string.Empty;

        public Usuario Usuario { get; set; } = null!;
        public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    }

}
