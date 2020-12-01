using System;
using System.Collections.Generic;
using System.Text;

namespace izzitech.Broker.Extensiones.AmbuBroker.Models
{
    public class Configuracion
    {
        public string CarpetaOrigen { get; set; }
        public string CarpetaDestino { get; set; }
        public string NombreOrigenRX { get; set; }
        public bool SobrescribirDestino { get; set; }
        public bool EliminarOrigen { get; set; }
        public string Filtro { get; set; }
    }
}
