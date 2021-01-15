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

        // El siguiente campo genera problemas con las Queries de EntityFramework Core.
        // [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [NotMapped]
        public DateTime? Fecha
        {
            get
            {
                return ParsearFechaAmbu(_Fecha, _Hora);
            }
        }
        // Estos campos son los que se utilizan para EntityFramework Core.
        [Column("fecha")]
        public DateTime? _Fecha { get; set; }

        [Column("hora")]
        public string _Hora { get; set; }

        // El siguiente campo genera problemas con las Queries de EntityFramework Core.
        [NotMapped]
        public DateTime? FechaEntregaInforme
        {
            get
            {
                return ParsearFechaAmbu(_FechaEntregaInforme, _HoraEntregaInforme);
            }
        }

        // Estos campos son los que se utilizan para EntityFramework Core.
        [Column("ent_inf_fe")]
        public DateTime? _FechaEntregaInforme { get; set; }

        [Column("ent_inf_ho")]
        public string _HoraEntregaInforme { get; set; }

        // El siguiente campo genera problemas con las Queries de EntityFramework Core.
        [NotMapped]
        public DateTime? FechaRecibeInforme
        {
            get
            {
                return ParsearFechaAmbu(_FechaRecibeInforme, _HoraRecibeInforme);
            }
        }

        // Estos campos son los que se utilizan para EntityFramework Core.
        [Column("rec_inf_fe")]
        public DateTime? _FechaRecibeInforme { get; set; }

        [Column("rec_inf_ho")]
        public string _HoraRecibeInforme { get; set; }

        // El siguiente campo genera problemas con las Queries de EntityFramework Core.
        [NotMapped]
        public DateTime? HorarioDeLlegada
        {
            get
            {
                // Del mal diseño del ambulatorio que no guarda fechas verdaderas, tenemos que asumir que el paciente
                // llegó el día del turno y no un día incorrecto.
                return ParsearFechaAmbu(_Fecha, _HoraLlega);
            }
        }

        // Este es el campo que se utilizan para EntityFramework Core.
        [Column("hora_llega")]
        public string _HoraLlega { get; set; }

        // El siguiente campo genera problemas con las Queries de EntityFramework Core.
        // Asumo que este es el horario del "PASA"
        [NotMapped]
        public DateTime? HorarioDeIngreso
        {
            get
            {
                // Del mal diseño del ambulatorio que no guarda fechas verdaderas, tenemos que asumir que el paciente
                // llegó el día del turno y no un día incorrecto.
                return ParsearFechaAmbu(_Fecha, _HoraIngresa);
            }
        }

        // Este es el campo que se utilizan para EntityFramework Core.
        [Column("hora_ingre")]
        public string _HoraIngresa { get; set; }

        /* El siguiente código genera error, para variar. Además no se usa.
        // El siguiente campo genera problemas con las Queries de EntityFramework Core.
        [NotMapped]
        public DateTime? HorarioDeSalida
        {
            get
            {
                // Del mal diseño del ambulatorio que no guarda fechas verdaderas, tenemos que asumir que el paciente
                // llegó el día del turno y no un día incorrecto.
                return ParsearFechaAmbu(_Fecha, _HoraSale);
            }
        }

        // Este es el campo que se utilizan para EntityFramework Core.
        [Column("hora_sale")]
        public string _HoraSale { get; set; }
        */

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

        [NotMapped]
        public bool PidioMedico { 
            get {
                if(!String.IsNullOrWhiteSpace(_PidioMedico) &&
                    _PidioMedico == "S")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set { _PidioMedico = value ? "S" : "N"; } }

        [Column("pidio_med")]
        public string _PidioMedico { get; set; }

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

        DateTime? ParsearFechaAmbu(DateTime? fecha, string hora)
        {
            if (fecha == null) return null;
            if (string.IsNullOrWhiteSpace(hora)) return fecha;

            fecha = fecha.Value.AddHours(double.Parse(hora.Substring(0, 2)));
            fecha = fecha.Value.AddMinutes(double.Parse(hora.Substring(2, 2)));
            return fecha;
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
            if (turno1.Fecha == null && turno2.Fecha == null) return 0;
            if (turno1.Fecha == null && turno2.Fecha != null) return -1;
            if (turno1.Fecha != null && turno2.Fecha == null) return 1;

            return turno1.Fecha.Value.CompareTo(turno2.Fecha.Value);
        }
    }
}
