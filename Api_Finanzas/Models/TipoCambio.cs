﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_Finanzas.Models
{
    [Table("TipoCambio")] 
    public class TipoCambio
    {
        [Key]
        public int TipoCambioId { get; set; }
        public string MonedaOrigen { get; set; }
        public string MonedaDestino { get; set; }
        public decimal Tasa { get; set; }
        public DateTime Fecha { get; set; }
    }
}
