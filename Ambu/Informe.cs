using System.IO;
using System.Text.RegularExpressions;

namespace izzitech.JST.Ambu
{
    public static class Informe
	{
		/// <summary>
		/// Obtener el numero de protocolo desde una ruta de archivo de informe.
		/// </summary>
		/// <param name="fullname">Ruta al informe con el nombre completo del archivo.</param>
		/// <returns></returns>
		public static long ParsearProtocolo(string fullname)
		{
			var archivo = new FileInfo(fullname);
			var extension = archivo.Extension;
			var titulo = archivo.Name.Remove(archivo.Name.Length - extension.Length);

			if (Regex.IsMatch(titulo, @"\d{8}"))
			{
				if (long.TryParse(titulo, out long protocolo))
				{
					return protocolo;
				}
				else
				{
					return 0;
				}
			}
            else
            {
				throw new System.Exception($"El nombre del archivo '{archivo.Name}' no está formateado correctamente.");
            }
		}

		/// <summary>
		/// Obtener la ruta completa en donde debería ser o estar guardado el informe \\UNC\xx-xxx\xxxxxxxx.doc a partir del protocolo.
		/// </summary>
		/// <param name="Unc">Ruta donde están almacenados los informes.</param>
		/// <param name="Protocolo">Número de acceso xxxxxxxx (por ej.: 18123456).</param>
		/// <returns>Ruta completa al informe.</returns>
		public static string Ruta(string Unc, long Protocolo)
		{
			var ruta = string.Format(
			"{0}-{1}", 
			Protocolo.ToString().Substring(0, 2), 
			Protocolo.ToString().Substring(2, 3));
			
			return Path.Combine(Unc, ruta, Protocolo.ToString() + ".doc"); ;
		}
	}
}
