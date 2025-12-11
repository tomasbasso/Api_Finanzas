using System.ComponentModel.DataAnnotations;

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
        // Saldo actual persistido; se mantiene al crear/editar/eliminar transacciones.
        public decimal SaldoActual { get; set; }
        public string TipoCuenta { get; set; } = string.Empty;

        public Usuario Usuario { get; set; } = null!;
        public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    }
}
