using System;

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
        public bool EvitarArchivosTemporales { get; set; }

        public static Configuracion ObtenerConfiguracionPorDefecto()
        {
            return new Configuracion()
            {
                CarpetaOrigen = AppContext.BaseDirectory,
                CarpetaDestino = AppContext.BaseDirectory,
                Filtro = "*",
                EvitarArchivosTemporales = true,
                SobrescribirDestino = false
            };
        }
    }
}
