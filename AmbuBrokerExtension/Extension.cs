using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using izzitech.Broker.Interfaces;
using izzitech.Broker.Extensiones.AmbuBroker.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using izzitech.FileUtils;
using izzitech.FileUtils;

namespace izzitech.Broker.Extensiones.AmbuBroker
{
    public class Extension : ITransportable
    {
        private int CantidadMaximaDeReintentos = 10;
        private Configuracion _config;
        private bool Corriendo = false;
        private Thread currentThread;
        private ConcurrentQueue<Pasajero> pasajeros = new ConcurrentQueue<Pasajero>();
        private NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private List<ArchivoProcesadoDelegado> suscriptosAlEventoProcesandoArchivo { get; set; }

        private void Trabajo()
        {
            while (Corriendo)
            {
                Pasajero pasajero;
                if (pasajeros.TryDequeue(out pasajero))
                {
                    try
                    {
                        CopiarArchivo(pasajero.Ruta);
                        pasajero.UltimoError = null;
                    }
                    catch (Exception ex)
                    {

                        _logger.Error(ex, $"No se pudo copiar el archivo {pasajero.Ruta} porque '{ex.Message}'. Se ha intentado: {pasajero.Intentos}. Se devuelve al transporte.");
                        pasajero.Intentos++;
                        pasajero.UltimoError = ex;

                        if (pasajero.Intentos < CantidadMaximaDeReintentos)
                        {
                            pasajeros.Enqueue(pasajero);
                        }
                        else
                        {
                            _logger.Error(ex, $"El pasajero {pasajero.Ruta} se elimina del transporte luego de {pasajero.Intentos} intentos.");
                        }
                    }
                }
                else
                {
                    // Disminuimos el consumo de CPU.
                    Thread.Sleep(100);
                }
            }
        }

        private void CopiarArchivo(string rutaArchivo)
        {
            var archivo = new FileInfo(rutaArchivo);
            if (!archivo.Exists) return;
            if (_config.EvitarArchivosTemporales) {
                if(archivo.Name.StartsWith("~")) return;
                if (archivo.Extension.Equals("bak")) return;
            }

            _logger.Debug($"Copiando el archivo {archivo.Name}");
            ProcesandoArchivoEvento(rutaArchivo);
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

            long protocolo = JST.Ambu.Informe.ParsearProtocolo(rutaArchivo);
            var archivoDestino = new FileInfo(JST.Ambu.Informe.Ruta(_config.CarpetaDestino, protocolo));
            Directory.CreateDirectory(archivoDestino.DirectoryName);


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


            try
            {
                if (_config.SobrescribirDestino)
                {
                    destino.HacerBak(true);
                }
                 archivo.CopyTo(archivoDestino.FullName, _config.SobrescribirDestino);

                if (_config.EliminarOrigen)
                {
                    try
                    {
                        origen.Delete();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, $"No se pudo eliminar el archivo {origen.FullName} porque '{ex.Message}'");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, $"ocurrió una excepción de Microsoft Word, porque {ex.Message}");
                pasajero.Intentos++;
                pasajero.UltimoError = ex;
                if (pasajero.Intentos < CantidadMaximaDeReintentos)
                {
                    pasajeros.Enqueue(pasajero);
                }
                else
                {
                    _logger.Error(ex, $"El pasajero {pasajero.Ruta} se elimina del transporte luego de {pasajero.Intentos} intentos. Por favor, cierre manualmente los procesos abiertos de Microsoft Word.");
                }
            }

        }

        public void Iniciar()
        {
            if (_config == null) throw new Exception("El transporte no esta configurado");
            Corriendo = true;
            var observador = new FileSystemWatcher(_config.CarpetaOrigen, _config.Filtro);
            observador.NotifyFilter =
                NotifyFilters.LastAccess |
                NotifyFilters.CreationTime |
                NotifyFilters.FileName;
            observador.Created += new FileSystemEventHandler(Observador_Changed);
            observador.Changed += new FileSystemEventHandler(Observador_Changed);
            observador.Renamed += new RenamedEventHandler(Observador_Renamed);
            observador.Error += new ErrorEventHandler(Observador_Error);
            observador.EnableRaisingEvents = true;

            currentThread = new Thread(new ThreadStart(Trabajo));
            currentThread.Start();
        }

        private void Observador_Renamed(object sender, RenamedEventArgs e)
        {
            var pasajero = new Pasajero()
            {
                Ruta = e.FullPath
            };

            pasajeros.Enqueue(pasajero);
        }

        private void Observador_Changed(object sender, FileSystemEventArgs e)
        {
            var pasajero = new Pasajero()
            {
                Ruta = e.FullPath
            };

            pasajeros.Enqueue(pasajero);
        }


        private void Observador_Error(object source, ErrorEventArgs e)
        {
            _logger.Error("El observador detecto un error");
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                //  This can happen if Windows is reporting many file system events quickly
                //  and internal buffer of the  FileSystemWatcher is not large enough to handle this
                //  rate of events. The InternalBufferOverflowException error informs the application
                //  that some of the file system events are being lost.
                _logger.Error("El observador sufrió un buffer overflow: " + e.GetException().Message);
            }
        }

        public void Detener()
        {
            Corriendo = false;
            currentThread.Join();
        }

        public void Configurar(IConfigurador configurador, string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    _config = configurador.Abrir<Models.Configuracion>(path);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "error al configurar el transporte");
                }
            }
            else
            {
                configurador.Guardar(path, ObtenerConfiguracionPorDefecto());
                throw new Exception("primero configure el transporte");
            }
        }

        private Models.Configuracion ObtenerConfiguracionPorDefecto()
        {
            return new Models.Configuracion()
            {
                CarpetaOrigen = AppContext.BaseDirectory,
                CarpetaDestino = AppContext.BaseDirectory,
                Filtro = "*",
                EvitarArchivosTemporales = true,
                SobrescribirDestino = false
            };
        }

        public void SuscribirAEventoArchivoProcesado(ArchivoProcesadoDelegado delegado)
        {
            if (suscriptosAlEventoProcesandoArchivo == null)
                suscriptosAlEventoProcesandoArchivo = new List<ArchivoProcesadoDelegado>();
            suscriptosAlEventoProcesandoArchivo.Add(delegado);
        }

        private void ProcesandoArchivoEvento(string archivo)
        {
            foreach (var suscripto in suscriptosAlEventoProcesandoArchivo)
            {
                suscripto(archivo);
            }
        }
    }
}
