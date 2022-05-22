using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Confluent.Kafka;
using System.Net.Sockets;
using System.Net;

namespace FWQ_Engine
{
    class FWQ_Engine
    {

        
        public static void Main(string[] args)
        {
            Socket servidor_TE = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint direccion = new IPEndPoint(IPAddress.Parse("172.27.101.136"), 8080);
            servidor_TE.Bind(direccion);
            servidor_TE.Listen(5);
            Socket Escuchar = servidor_TE.Accept();

            String[,] map = new string[20, 20];
            for (int i=0;i<20;i++)
            {
                for (int j=0;j<20;j++)
                {
                    map[i, j] = ". ";
                }
            }



            SQLiteConnection cadenaconexionP = new SQLiteConnection("Data Source = C:/Users/aleja/OneDrive/Escritorio/general.sqlite");
            cadenaconexionP.Open();

            string consultaP = "select * from parque";
            SQLiteCommand comandoP = new SQLiteCommand(consultaP, cadenaconexionP);
            SQLiteDataReader datosP = comandoP.ExecuteReader();

            string devolver = "/";

            while (datosP.Read())
            {
                int colaMAX = datosP.GetInt32(6);
                int duracion = datosP.GetInt32(5);
                int aforo = datosP.GetInt32(4);
                int Cola = 0;
                string nombre = datosP.GetString(0);
                Console.WriteLine("Waiting for " + nombre);
                string poner = comenzarEscucha(nombre,servidor_TE,Escuchar);
                if (poner == "")
                {
                    Cola= datosP.GetInt32(3);
                }
                else
                {
                    Cola = Int32.Parse(poner);
                }
                int CX = datosP.GetInt32(1);
                int CY = datosP.GetInt32(2);
                int totalCola = (duracion * Cola) / aforo;
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        if(CX==i && CY == j)
                        {
                            if (colaMAX > Cola)
                            {
                                map[i, j] = totalCola.ToString();
                                devolver = devolver + " A " + totalCola.ToString() + " " + CX.ToString() + " " + CY.ToString();
                            }
                            else
                            {
                                map[i, j] = totalCola.ToString();
                                devolver = devolver + " A " + Cola.ToString() + " " + CX.ToString() + " " + CY.ToString();
                            }
                            
                        }
                    }
                }

            }

            String recibido = "";
            int opcion=0;
            String bootstrapServ = "";
            int posXUser = 0;
            int posYUser = 0;
            String nombreTopic = "";
            string nombreUser = "";
            if (args.Length == 0)
            {
                Console.WriteLine("Introduce el bootstrapServer: ");
                bootstrapServ = Console.ReadLine();
                Console.WriteLine("Introduce el nombre del topic: ");
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
            }
            var config = new ConsumerConfig
            {

                BootstrapServers = bootstrapServ,
                GroupId = "console-consumer-43044"

            };

            var config2 = new ProducerConfig
            {
                BootstrapServers = bootstrapServ,
            };

            do
            {
                using (var consumer = new ConsumerBuilder<Null, string>(config)
                    .Build())
                {
                    Console.WriteLine("Esperando tus datos");
                    consumer.Subscribe(nombreTopic);
                    ConsumeResult<Null, string> consumeResult = consumer.Consume();
                    recibido = consumeResult.Value;
                }
                Console.WriteLine("Obtenidos los datos: " + recibido);
                string[] cadRecibido = recibido.Split(' ');
                SQLiteConnection cadenaconexion = new SQLiteConnection("Data Source = C:/Users/aleja/OneDrive/Escritorio/general.sqlite");
                cadenaconexion.Open();
                string consulta = "";
                consulta = "select nombre, posX, posY from usuarios where alias like '" + cadRecibido[1] + "' and password like '" + cadRecibido[2] + "'";
                SQLiteCommand comando = new SQLiteCommand(consulta, cadenaconexion);
                SQLiteDataReader datos = comando.ExecuteReader();

                while (datos.Read())
                {
                    nombreUser = datos.GetString(0);
                    posXUser = datos.GetInt32(1);
                    posYUser = datos.GetInt32(2);
                }
                datos.Close();
                Console.WriteLine(nombreUser);
                if (nombreUser == "")
                {
                    using (var producer = new ProducerBuilder<Null, string>(config2).Build())
                    {
                        producer.Produce(nombreTopic, new Message<Null, string> { Value = "Error al introducir los datos" });
                        producer.Flush(TimeSpan.FromSeconds(2));
                    }
                }
                else
                {
                    opcion = int.Parse(cadRecibido[0]);
                    int destino = 99999;
                    int XDestino = 0;
                    int YDestino = 0;
                    if (opcion == 3)  //AQUIIIIIIIIIIIIIIIIIIIIIII
                    {
                        Console.WriteLine("Opcion 3 procesando");
                        string[] partidos = devolver.Split('/');
                        string[] users = partidos[0].Split(' ');
                        string[] atracc = partidos[1].Split(' ');
                        string mandar="";
                        foreach (string hola in atracc){
                            mandar += hola;
                        }
                        devolver = "/ " + partidos[1];
                        for(int i = 0; i < atracc.Length; i++)
                        {
                            
                            if(atracc[i]=="A" && destino > int.Parse(atracc[i + 1]))
                            { 
                                Console.WriteLine("Entra");
                                destino = int.Parse(atracc[i + 1]);
                                XDestino = int.Parse(atracc[i + 2]);
                                YDestino = int.Parse(atracc[i + 3]);
                                using (var producer = new ProducerBuilder<Null, string>(config2).Build())
                                {
                                    Console.WriteLine(nombreTopic);
                                    producer.Produce(nombreTopic, new Message<Null, string> { Value = mandar });
                                    producer.Flush(TimeSpan.FromSeconds(2));

                                }
                            }
                        }
                    }   //ACABAAAAAAAAAAA
                    else if(opcion == 4)
                    {
                        string[] partidos = devolver.Split('/');
                        string[] users = partidos[0].Split(' ');
                        devolver = "/ " + partidos[1];
                        for(int i = 0; i < users.Length; i++)
                        {
                            if (users[i] == "U" && posXUser.ToString() == users[i+1] && posYUser.ToString() == users[i+2])
                            {
                                i = i + 2;
                            }
                            else
                            {
                                devolver = users[i] + " " + devolver;
                            }
                        }
                        using (var producer = new ProducerBuilder<Null, string>(config2).Build())
                        {
                            producer.Produce(nombreTopic, new Message<Null, string> { Value = devolver });
                            producer.Flush(TimeSpan.FromSeconds(2));
                        }
                    }
                    else
                    {
                        using (var producer = new ProducerBuilder<Null, string>(config2).Build())
                        {
                            producer.Produce(nombreTopic, new Message<Null, string> { Value = "Error opcion invalida" });
                            producer.Flush(TimeSpan.FromSeconds(2));
                        }
                    }
                }
            } while (true);
            
            /*
            string[,] mapa = new string[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    mapa[i, j] = ".";
                }
            }
            SQLiteConnection cadenaconexion = new SQLiteConnection("Data Source = C:/Users/aleja/OneDrive/Escritorio/general.sqlite");
            cadenaconexion.Open();

            string consulta = "select * from parque";
            SQLiteCommand comando = new SQLiteCommand(consulta, cadenaconexion);
            SQLiteDataReader datos = comando.ExecuteReader();
            
            while (datos.Read())
            {
                int Cola = datos.GetInt32(3);
                int colaMAX = datos.GetInt32(6);
                int duracion = datos.GetInt32(5);
                int aforo = datos.GetInt32(4);
                int totalCola = (duracion * Cola)/aforo;
                string nombre = datos.GetString(0);
                int CX = datos.GetInt32(1);
                int CY = datos.GetInt32(2);

                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        if (i == CX && j == CY)
                        {
                            mapa[i, j] = "A";
                        }
                    }
                }
            }
            

           // Console.ReadKey();
            //cadenaconexion.Close();

            SQLiteConnection cadenaconexion2 = new SQLiteConnection("Data Source = C:/Users/aleja/OneDrive/Escritorio/general.sqlite");
            cadenaconexion2.Open();

            string consulta2 = "select * from usuarios";
            SQLiteCommand comando2 = new SQLiteCommand(consulta2, cadenaconexion2);
            SQLiteDataReader datos2 = comando2.ExecuteReader();

            while (datos2.Read())
            {
                int PX = datos2.GetInt32(3);
                int PY = datos2.GetInt32(4);
                string nombre = datos2.GetString(0);

                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        if (i == PX && j == PY)
                        {
                            mapa[i, j] = "X";
                        }
                    }
                }
            }

            //cd Console.ReadKey();
            cadenaconexion2.Close();
            
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

            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    Console.Write(mapa[i, j]);
                }
                Console.WriteLine("");
            }

            var config = new ProducerConfig
            {

                BootstrapServers = bootstrapServ,

            };

            Action<DeliveryReport<Null, string[,]>> handler = r =>
            Console.WriteLine(!r.Error.IsError
            ? $"Delivered message to {r.TopicPartitionOffset}"
            : $"Delivery Error: {r.Error.Reason}");

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                for(int i = 0; i < 20; i++)
                {
                    for(int j=0; j < 20; j++)
                    {
                        producer.Produce(nombreTopic, new Message<Null, string> { Value = mapa[i,j] });
                        producer.Flush(TimeSpan.FromSeconds(10));
                    }
                }
                
            }*/
        }
        public static string comenzarEscucha(string nombreAt, Socket servidor_TE, Socket Escuchar)
        {
            


            

            Console.WriteLine("Escuchando...");

            

            bool acabar = true;

            Console.WriteLine("Conectado con exito para encontrar "+nombreAt);

            while (acabar)
            {

                byte[] ByRec = new byte[255];

                int a = Escuchar.Receive(ByRec, 0, ByRec.Length, 0);

                Array.Resize(ref ByRec, a);

                Console.WriteLine("Cliente dice: " + Encoding.Default.GetString(ByRec)); //mostramos lo recibido

                string[] resultado = Encoding.Default.GetString(ByRec).Split(' ');

                if (nombreAt == resultado[0])
                {
                    acabar = false;
                    return resultado[1];
                    
                }

            }

            servidor_TE.Close();

            Console.WriteLine("Presione cualquier tecla para terminar");

            Console.ReadKey();

            return "";
        }
    }
}