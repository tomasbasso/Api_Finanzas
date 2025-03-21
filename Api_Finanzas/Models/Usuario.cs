namespace Api_Finanzas.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string ContrasenaHash { get; set; }
        public DateTime FechaRegistro { get; set; }

        public ICollection<CuentaBancaria> Cuentas { get; set; }
        public ICollection<Transaccion> Transacciones { get; set; }
        public ICollection<Presupuesto> Presupuestos { get; set; }
        public ICollection<MetaAhorro> MetasAhorro { get; set; }
        public ICollection<Alerta> Alertas { get; set; }
    }

}
