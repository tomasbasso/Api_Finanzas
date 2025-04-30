﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api_Finanzas.Models
{
        public class Presupuesto
        {
            [Key]
            public int PresupuestoId { get; set; }

            public int UsuarioId { get; set; }
            public int CategoriaGastoId { get; set; }

            public decimal MontoLimite { get; set; }
            public int Mes { get; set; }
            public int Año { get; set; }

            [ForeignKey(nameof(UsuarioId))]
            public Usuario Usuario { get; set; }

            [ForeignKey(nameof(CategoriaGastoId))]
            public CategoriaGasto CategoriaGasto { get; set; }

        }
    
}
