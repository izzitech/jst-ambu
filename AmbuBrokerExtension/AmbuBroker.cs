using izzitech.Broker.Interfaces;
using izzitech.izzilib;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace izzitech.Broker.Extensiones.AmbuBroker
{
    public class AmbuBroker : ITransportable
    {
        Models.Configuracion _config;
        bool Corriendo = false;
        Thread currentThread;
        ConcurrentQueue<string> cola = new ConcurrentQueue<string>();
        NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private void Trabajo()
        {
            while (Corriendo)
            {
                string rutaArchivo;
                if (cola.TryDequeue(out rutaArchivo))
                {
                    MoverInforme(rutaArchivo);
                }
                Thread.Sleep(100);
            }
        }

        private void MoverInforme(string rutaArchivo)
        {
            var archivo = new FileInfo(rutaArchivo);

            _logger.Debug($"Renombrando el archivo {archivo.Name}");

            try
            {
                while (FileUtils.IsLocked(archivo.FullName)) ;

                // Renombrar.

                // Crear ruta al ambu con AmbuUtils.

                archivo.CopyTo(Path.Combine(_config.CarpetaDestino, archivo.Name), _config.SobrescribirDestino);
                if (_config.EliminarOrigen)
                {
                    try
                    {
                        if (archivo.Exists)
                        {
                            while (FileUtils.IsLocked(archivo.FullName)) ;
                            archivo.Delete();
                        }
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
            catch (Exception ex)
            {
                _logger.Error(ex, $"No se pudo copiar el archivo {archivo.Name} porque '{ex.Message}'");
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
            // observador.Changed += new FileSystemEventHandler(Observador_Changed);
            observador.Error += new ErrorEventHandler(Observador_Error);
            observador.EnableRaisingEvents = true;

            currentThread = new Thread(new ThreadStart(Trabajo));
            currentThread.Start();
        }

        private void Observador_Changed(object sender, FileSystemEventArgs e)
        {
            cola.Enqueue(e.FullPath);
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
                SobrescribirDestino = false
            };
        }
    }
}
