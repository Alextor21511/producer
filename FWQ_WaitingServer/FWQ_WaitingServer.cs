using Confluent.Kafka;
using System;
using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously  
public class StateObject
{
    // Size of receive buffer.  
    public const int BufferSize = 1024;

    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];

    // Received data string.
    public StringBuilder sb = new StringBuilder();

    // Client socket.
    public Socket workSocket = null;
}

class FWQ_WaitingServer {
    static void Main(string[] args)
    {
        string bootstrapServ = "";
        string nombreTopic = "";
        string nombreAtraccion = "";
        int numCola = 0;

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

        var config = new ConsumerConfig
        {

            BootstrapServers = bootstrapServ,
            GroupId = "console-consumer-43044"

        };
        Socket conectar_SE = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPEndPoint direccion = new IPEndPoint(IPAddress.Parse("172.27.101.136"), 8080);

        conectar_SE.Connect(direccion);

        Console.WriteLine("CONECTADO");

        String resultado = "";
        using (var consumer = new ConsumerBuilder<Null, string>(config)
       .Build())
        {
            consumer.Subscribe(nombreTopic);
            while (true) {
                ConsumeResult<Null, string> consumeResult = consumer.Consume();
                Console.WriteLine(consumeResult.Value);
                string respuesta = consumeResult.Value;

                byte[] enviar_respuesta = Encoding.Default.GetBytes(respuesta);

                conectar_SE.Send(enviar_respuesta, 0, enviar_respuesta.Length, 0);
                Thread.Sleep(200);
            }
            
            
            //numCola = datos[1];

        }

   
}
    

}



