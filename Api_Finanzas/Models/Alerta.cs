using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.Models
{
    public class Alerta
    {
        [Key]
        public int AlertaId { get; set; }
        public int UsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Condicion { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;

    public Usuario Usuario { get; set; } = null!;
    }

}
