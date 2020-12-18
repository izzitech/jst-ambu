using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace izzitech.Broker.Extensiones.AmbuBroker.Models
{
    class Pasajero
    {
        public string Ruta { get; set; }
        public int Intentos { get; set; }
        public Exception UltimoError { get; set; }
    }
}
