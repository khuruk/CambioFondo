using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;


namespace Automatico
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction,
        int uParam, string lpvParam, int fuWinIni);

        private static readonly int MAX_PATH = 260;
        private static readonly int SPI_GETDESKWALLPAPER = 0x73;
        private static readonly int SPI_SETDESKWALLPAPER = 0x14;
        private static readonly int SPIF_UPDATEINIFILE = 0x01;
        private static readonly int SPIF_SENDWININICHANGE = 0x02;

        static string obtenerFondoPantallaActual()
        {
            string wallpaper = new string('\0', MAX_PATH);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, (int)wallpaper.Length, wallpaper, 0);
            return wallpaper.Substring(0, wallpaper.IndexOf('\0'));
        }

        static void cambiarFondoPantalla(string filename)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filename,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
        static void Main(string[] args)
        {
            try
            {
                //recuperamos J
                var jsonEstadoS = new WebClient().DownloadString("https://pastebin.com/raw/NuHhT7uZ").ToString();
                dynamic jsonEstado = JsonConvert.DeserializeObject(jsonEstadoS);
                bool activoEstado = (bool)jsonEstado.SelectToken("activo");
                Dictionary<string, bool> registroCambio = new Dictionary<string, bool>();
                while (activoEstado)
                {                    
                    var str = new WebClient().DownloadString("https://pastebin.com/raw/MKautkrG").ToString();
                    dynamic json = JsonConvert.DeserializeObject(str);
                    int contador = 0;
                    JArray displayName = json.SelectToken("nacho");
                    bool activo = (bool)json.SelectToken("activo");
                    bool estado = (bool)json.SelectToken("estado");
                    if (activo)
                    {
                        //comprueba fecha 
                        DateTime ahora = DateTime.Now;
                        string fechaEjecucion = ahora.Day.ToString() + ahora.Month.ToString() + ahora.Year + " " + ahora.Hour.ToString() + ahora.Minute;
                        string horaActual = ahora.Hour.ToString();

                        if (horaActual.Length == 1) horaActual = "0" + horaActual;

                        string minutosActual = ahora.Minute.ToString();
                        if (minutosActual.Length == 1) minutosActual = "0" + minutosActual;
                        string horaLocal = horaActual + ":" + minutosActual;
                        JArray horaArray = json.SelectToken("hora");
                        //comprobamos si ya hemos cambiado el fondo en esta fecha y minutos
                        bool ejecucion = true;
                        if (registroCambio.ContainsKey(fechaEjecucion))
                        {
                            ejecucion = registroCambio[fechaEjecucion];
                        }
                        if (ejecucion)
                        {
                            foreach (var hora in horaArray)
                            {
                                //fecha correcta
                                if (hora.ToString().Equals(horaLocal))
                                {
                                    //url random
                                    Random random = new Random();
                                    int randomNumber = random.Next(0, (displayName.Count - 1));
                                    foreach (var urls in displayName)
                                    {

                                        if (contador == randomNumber)
                                        {
                                            //cambio de img
                                            string url = urls.ToString();
                                            string localFilename = @"C:\Users\Public\Pictures\fondoPantalla.jpg";
                                            using (WebClient client = new WebClient())
                                            {
                                                client.DownloadFile(url, localFilename);
                                                //client.DownloadFile("https://www.dropbox.com/s/ucswqul0w6oldr4/juegomental.jpg?raw=1", localFilename);
                                                cambiarFondoPantalla(localFilename);
                                                registroCambio.Add(fechaEjecucion, false);
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            contador++;
                                        }
                                    }
                                }
                            }
                        }

                        //string actual = obtenerFondoPantallaActual();
                    }
                    activoEstado = estado;

                }
                Environment.Exit(-1);
            }
            catch (IndexOutOfRangeException ex)
            {

            }
        }

    }
}
