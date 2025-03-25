using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.Models
{
    public class CuentaBancaria
    {
        [Key]
        public int CuentaId { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Banco { get; set; }
        public decimal SaldoInicial { get; set; }
        public string TipoCuenta { get; set; }

        public Usuario Usuario { get; set; }
        public ICollection<Transaccion> Transacciones { get; set; }
    }

}
