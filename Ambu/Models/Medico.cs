using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace izzitech.JST.Ambu.Models
{
    [Table("tu_med")]
    public class Medico
    {
        public long Id { get; set; }

        public string Nombre { get; set; }

        public int Matricula { get; set; }

        public string Direccion { get; set; }
        public string Cp { get; set; }
        public string Telefono { get; set; }

        public string Dni { get; set; }
        public string Cuit { get; set; }

        [Column("especialid")]
        public string Especialidad { get; set; }

        public string Email { get; set; }

        [Column("nacimiento")]
        private string _nacimiento { get; set; }

        public DateTime FechaDeNacimiento
        {
            get
            {
                return DateTime.Parse(_nacimiento);
            }
        }
        public string Matricula2 { get; set; }

        public string Categoria { get; set; }

        public string Nota { get; set; }

        public string Nota2 { get; set; }
    }
}
