using izzitech.Broker.Configurador;
using izzitech.Broker.Transportes;
using izzitech.Broker.Transportes.Models;
using izzitech.FileUtils;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Transportes.Delegados;

namespace izzitech.Broker.Extensiones.AmbuBroker
{
    public class CopiarInformesDelAmbu : Transporte
    {
        private const string RegexPattern = @"^\d{8}.doc$";
        private Models.Configuracion _config { get; set; }

        public CopiarInformesDelAmbu(string nombre, string rutaDeLaConfiguracion, IConfigurador configurador)
        : base(nombre, rutaDeLaConfiguracion, configurador)
        {
            _logger = NLog.LogManager.GetLogger(GetType().FullName);
            Configurar();
        }

        private void Procesar(Pasajero pasajero)
        {
            if (ElPasajeroEsValido(pasajero))
            {
                CopiarPasajero(pasajero);
                pasajero.UltimoError = null;
            }
            else
            {
                _logger.Warn($"El pasajero {pasajero.Origen} no es valido para el transporte {GetType().Name}.");
            }
        }
        private bool ElPasajeroEsValido(Pasajero pasajero)
        {
            var fileInfo = new FileInfo(pasajero.Origen);
            return Regex.IsMatch(fileInfo.Name, RegexPattern);
        }

        private void CopiarPasajero(Pasajero pasajero)
        {
            var archivo = new FileInfo(pasajero.Origen);
            if (!archivo.Exists) return;
            if (_config.EvitarArchivosTemporales)
            {
                if (archivo.Name.StartsWith("~")) return;
                if (archivo.Extension.Equals("bak")) return;
                _logger.Warn("El pasajero es temporal así que no se copia.");
            }

            _logger.Debug($"Copiando el archivo {archivo.Name}");
            if (archivo.EstaBloqueado())
            {
                int reintentosPorBloqueo = 0;
                while (reintentosPorBloqueo++ < CantidadMaximaDeReintentos)
                {
                    Thread.Sleep(100);
                }
                if (archivo.EstaBloqueado())
                {
                    throw new Exception("el archivo de origen está bloqueado");
                }
            }

            long protocolo = JST.Ambu.Informe.ParsearProtocolo(pasajero.Origen);
            var archivoDestino = new FileInfo(JST.Ambu.Informe.Ruta(_config.CarpetaDestino, protocolo));

            _logger.Debug($"Creando el directorio {archivoDestino.DirectoryName}");
            Directory.CreateDirectory(archivoDestino.DirectoryName);

            if (_config.SobrescribirDestino)
            {
                archivoDestino.HacerBak(true);
            }

            _logger.Debug($"Copiando {archivoDestino.FullName}");
            archivo.CopyTo(archivoDestino.FullName, _config.SobrescribirDestino);
            PasajeroProcesado(pasajero);


            if (_config.EliminarOrigen)
            {
                try
                {
                    archivo.Delete();
                    _logger.Info($"Se ha movido el archivo {archivo.Name}");
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, $"No se pudo eliminar el archivo {archivo.FullName} porque '{ex.Message}'");
                }
            }
            else
            {
                _logger.Info($"Se ha copiado el archivo {archivo.Name}");
            }
        }

        public override void Iniciar()
        {
            if (Estado.Equals(Estado.Preparado) || Estado.Equals(Estado.Detenido))
            {
                AsignarEstado(Estado.Iniciando);
                if (_config == null)
                {
                    AsignarEstado(Estado.Error);
                    throw new Exception("El transporte no esta configurado.");
                }

                var observador = new FileSystemWatcher(_config.CarpetaOrigen, _config.Filtro);
                observador.NotifyFilter =
                    NotifyFilters.LastAccess |
                    NotifyFilters.CreationTime |
                    NotifyFilters.FileName;

                var delegado = new ProcesarPasajeroDelegado(Procesar);
                ActivarObservador(observador);
                ActivarProcesamiento(delegado);
                AsignarEstado(Estado.Trabajando);
                _logger.Info($"Iniciada la extensión {GetType().Name}.");
            }
            else
            {
                _logger.Info($"La extensión {GetType().Name} no se pudo iniciar su estado es {Estado}.");
            }
        }

        public void Configurar()
        {
            if (File.Exists(RutaDeLaConfiguracion))
            {
                try
                {
                    _config =
                        Configurador
                        .Abrir<Models.Configuracion>(
                            RutaDeLaConfiguracion);
                }
                catch (Exception ex)
                {
                    AsignarEstado(Estado.Error);
                    throw ex;
                }
            }
            else
            {
                Configurador
                    .Guardar(
                    RutaDeLaConfiguracion,
                    Models.Configuracion.ObtenerConfiguracionPorDefecto());

                AsignarEstado(Estado.Error);
                throw new Exception("primero configure el transporte");
            }
        }
    }
}
