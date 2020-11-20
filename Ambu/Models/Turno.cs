using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace izzitech.JST.Ambu.Models
{
    [Table("turno")]
    public class Turno : IComparable
    {
        public long Id { get; set; }

        public string Paciente { get; set; }

        public long Practica { get; set; }

        public double Peso { get; set; }

        // [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Fecha
        {
            get
            {
                return ParsearFechaAmbu(_Fecha, _Hora);
            }
        }

        [Column("fecha")]
        public string _Fecha { get; set; }

        [Column("hora")]
        public string _Hora { get; set; }

        public DateTime FechaEntregaInforme
        {
            get
            {
                return ParsearFechaAmbu(_FechaEntregaInforme, _HoraEntregaInforme);
            }
        }

        [Column("ent_inf_fe")]
        public string _FechaEntregaInforme { get; set; }

        [Column("ent_inf_ho")]
        public string _HoraEntregaInforme { get; set; }


        public DateTime FechaRecibeInforme
        {
            get
            {
                return ParsearFechaAmbu(_FechaEntregaInforme, _HoraEntregaInforme);
            }
        }

        [Column("rec_inf_fe")]
        public string _FechaRecibeInforme { get; set; }

        [Column("rec_inf_ho")]
        public string _HoraRecibeInforme { get; set; }

        [Column("dni")]
        public long Dni { get; set; }

        // [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Column("nacio")]
        public DateTime? Nacimiento { get; set; }

        // [Display(Name = "ID médico referente")]
        [Column("med_solic")]
        public int MedicoReferenteId { get; set; }

        // [Display(Name = "Médico referente")]
        [Column("nmed_solic")]
        public string MedicoReferenteNombre { get; set; }

        public string Equipo { get; set; }
        public string Estudios { get; set; }
        public string Estudio2 { get; set; }
        public string Estudio3 { get; set; }
        public string Estudio4 { get; set; }
        public string Estudio5 { get; set; }
        public string Estudio6 { get; set; }
        public string Estudio7 { get; set; }
        public string Estudio8 { get; set; }
        public string Estudio9 { get; set; }
        public string Estudio10 { get; set; }

        public string Localidad { get; set; }

        // [Display(Name = "Género")]
        [Column("sexo")]
        public string Genero { get; set; }

        // [Display(Name = "Matrícula")]
        public int Matricula { get; set; }

        // [Display(Name = "Médico informante")]

        // [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        // [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        // [Display(Name = "Obra social")]
        public string Obrasocial { get; set; }
        public string Nbeneos { get; set; }

        public string Nota { get; set; }
        public string Nota2 { get; set; }

        // [Display(Name = "Atención de")]
        [Column("clave")]
        public string AtendidoPor { get; set; }

        // [Display(Name = "eMail")]
        public string Email { get; set; }

        DateTime ParsearFechaAmbu(string fecha, string hora)
        {
            var dateTime = new DateTime();
            dateTime = DateTime.Parse(fecha);
            dateTime = dateTime.AddHours(double.Parse(hora.Substring(0, 2)));
            dateTime = dateTime.AddMinutes(double.Parse(hora.Substring(2, 2)));
            return dateTime;
        }

        int IComparable.CompareTo(object obj)
        {
            Turno turnoAComparar = (Turno)obj;

            return string.Compare(Paciente, turnoAComparar.Paciente);
        }
    }

    public class TurnoOrdenarPorFecha : IComparer<Turno>
    {
        public int Compare(Turno turno1, Turno turno2)
        {
            return turno1.Fecha.CompareTo(turno2.Fecha);
        }
    }
}
