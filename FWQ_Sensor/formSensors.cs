/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SQLite;
using Confluent.Kafka;

namespace form_Sensors
{
    class formSensors
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string bootstrapServ = "";
            string nombreTopic = "";
            //En caso de que no se introduzcan valores como argumento, cogemos la configuracion por defecto:
            if (args.Length == 0)
            {
                Console.Write("Por favor, introduce el bootstrap Server: ");
                bootstrapServ = Console.ReadLine();
                Console.Write("Introduzca el nombre del topic: ");
                nombreTopic = Console.ReadLine();
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--bootstrap-server")
                    {
                        bootstrapServ = args[i + 1];
                    }
                    if (args[i] == "--topic")
                    {
                        nombreTopic = args[i + 1];
                    }
                }
                if (bootstrapServ == "")
                {
                    Console.Write("Por favor, introduce el bootstrap Server: ");
                    bootstrapServ = Console.ReadLine();
                }
                if (nombreTopic == "")
                {
                    Console.Write("Introduzca el nombre del topic: ");
                    nombreTopic = Console.ReadLine();
                }
            }
            var config = new ProducerConfig
            {

                BootstrapServers = bootstrapServ,

            };

            Action<DeliveryReport<Null, string>> handler = r =>
            Console.WriteLine(!r.Error.IsError
            ? $"Delivered message to {r.TopicPartitionOffset}"
            : $"Delivery Error: {r.Error.Reason}");

            formSensors pr = new formSensors();
            String nombre=pr.getNombre();
            int numPersonas = pr.getNumPersonas();
            int esperaPersona = pr.getEsperaPersona();
            int totalEspera = numPersonas * esperaPersona;

            pr.mandarNombre(config,nombreTopic, nombre, totalEspera);
            
        }
        
        public void mandarNombre(ProducerConfig config,String nombreTopic, String nombreDato, int totalEspera)
        {
            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                String texto;
                producer.Produce(nombreTopic, new Message<Null, string> { Value = nombreDato });
                producer.Flush(TimeSpan.FromSeconds(10));
            }
        }
        public string getNombre()
        {
            string nombre = "";
            SQLiteConnection cadenaConexion = new SQLiteConnection("Data Source = C:/Users/aleja/OneDrive/Escritorio/SDPRUEBAS/SQLITE/atraccionesPerf.sqlite");
            cadenaConexion.Open();
            string consulta = "select * from atracciones";
            SQLiteCommand comando = new SQLiteCommand(consulta, cadenaConexion);
            SQLiteDataReader datos = comando.ExecuteReader();
            while (datos.Read())
            {
                nombre = Convert.ToString(datos[0]);
            }
            //ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            cadenaConexion.Close();
            return nombre;
        }

        public int getEsperaPersona()
        {
            int espPersona= 0;
            SQLiteConnection cadenaConexion = new SQLiteConnection("Data Source = C:/Users/aleja/OneDrive/Escritorio/SDPRUEBAS/SQLITE/atraccionesPerf.sqlite");
            cadenaConexion.Open();
            string consulta = "select * from atracciones";
            SQLiteCommand comando = new SQLiteCommand(consulta, cadenaConexion);
            SQLiteDataReader datos = comando.ExecuteReader();
            while (datos.Read())
            {
                espPersona = Convert.ToInt32(datos[2]);
            }
            //ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            cadenaConexion.Close();
            return espPersona;
        }
        public int getNumPersonas()
        {
            int numPersonas = 0;
            SQLiteConnection cadenaConexion = new SQLiteConnection("Data Source = C:/Users/aleja/OneDrive/Escritorio/SDPRUEBAS/SQLITE/atraccionesPerf.sqlite");
            cadenaConexion.Open();
            string consulta = "select * from atracciones";
            SQLiteCommand comando = new SQLiteCommand(consulta, cadenaConexion);
            SQLiteDataReader datos = comando.ExecuteReader();
            while (datos.Read())
            {
                numPersonas = Convert.ToInt32(datos[1]);
            }
            //ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            cadenaConexion.Close();
            return numPersonas;
        }

        public int setNumPersonas(int numeroNuevo, String nombreAtraccion)
        {
            int numPersonas = 0;
            SQLiteConnection cadenaConexion = new SQLiteConnection("Data Source = C:/Users/aleja/OneDrive/Escritorio/SDPRUEBAS/SQLITE/atraccionesPerf.sqlite");
            cadenaConexion.Open();
            string consulta = "update * from atracciones";
            SQLiteCommand comando = new SQLiteCommand(consulta, cadenaConexion);
            SQLiteDataReader datos = comando.ExecuteReader();
            while (datos.Read())
            {
                numPersonas = Convert.ToInt32(datos[1]);
            }
            //ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            cadenaConexion.Close();
            return numPersonas;
        }
    }
}*/
