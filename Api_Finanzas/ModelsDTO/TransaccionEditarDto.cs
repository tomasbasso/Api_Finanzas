// ModelsDTO/TransaccionEditarDto.cs
using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.ModelsDTO
{
    public class TransaccionEditarDto
    {
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Monto { get; init; }

        [Required, RegularExpression("Gasto|Ingreso")]
        public string Tipo { get; init; } = "Gasto";

        public int? CategoriaId { get; init; }

        [Required]
        public DateTime Fecha { get; init; }   // 👈 DateTime (no DateOnly)

        [Required]
        public int CuentaId { get; init; }

        public string? Descripcion { get; init; }
    }
}
