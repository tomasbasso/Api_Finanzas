using System.ComponentModel.DataAnnotations;

namespace Api_Finanzas.Models
{
    public class Alerta
    {
        [Key]
        public int AlertaId { get; set; }
        public int UsuarioId { get; set; }
        public string Tipo { get; set; }
        public string Condicion { get; set; }
        public string Mensaje { get; set; }

        public Usuario Usuario { get; set; }
    }

}
