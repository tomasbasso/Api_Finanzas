namespace Api_Finanzas.Models
{
    public class Transaccion
    {
        public int TransaccionId { get; set; }
        public int UsuarioId { get; set; }
        public int CuentaId { get; set; }
        public int? CategoriaGastoId { get; set; }
        public int? CategoriaIngresoId { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } // "Gasto" o "Ingreso"
        public string Descripcion { get; set; }

        public Usuario Usuario { get; set; }
        public CuentaBancaria Cuenta { get; set; }
        public CategoriaGasto CategoriaGasto { get; set; }
        public CategoriaIngreso CategoriaIngreso { get; set; }
    }

}
