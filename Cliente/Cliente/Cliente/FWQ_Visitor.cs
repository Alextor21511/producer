using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Confluent.Kafka;
using System.Text;

// State object for receiving data from remote device.  
public class StateObject
{
    // Client socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 256;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}

public class AsynchronousClient
{
    // The port number for the remote device.  
    private const int port = 11000;

    // ManualResetEvent instances signal completion.  
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);

    // The response from the remote device.  
    private static String response = String.Empty;

    private static void StartClient(int respuesta)
    {
        // Conectamos al registro  
        try
        {
            // Establish the remote endpoint for the socket.  
            // The name of the
            // remote device is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();

            // Send test data to the remote device.  

            String s;

            Console.WriteLine("Nombre de usuario: ");
            s= Console.ReadLine() + ":";
            Console.WriteLine("Alias de usuario: ");
            s += Console.ReadLine() + ":";
            Console.WriteLine("Password de usuario: ");
            s += Console.ReadLine();

            if (respuesta==1)
            {
                s += ":" + "1:<EOF>";
            }
            else if (respuesta == 2)
            {
                s += ":" + "2:<EOF>";
            }

            Console.WriteLine(s);

            Send(client, s);
            sendDone.WaitOne();

            // Receive the response from the remote device.  
            Receive(client);
            receiveDone.WaitOne();

            // Write the response to the console.  
            Console.WriteLine("Response received : {0}", response);

            // Release the socket.  
            client.Shutdown(SocketShutdown.Both);
            client.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);

            Console.WriteLine("Socket connected to {0}",
                client.RemoteEndPoint.ToString());

            // Signal that the connection has been made.  
            connectDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Receive(Socket client)
    {
        try
        {
            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.  
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the client socket
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            // Read data from the remote device.  
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Get the rest of the data.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                // All the data has arrived; put it in response.  
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }
                // Signal that all bytes have been received.  
                receiveDone.Set();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Send(Socket client, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to server.", bytesSent);

            // Signal that all bytes have been sent.  
            sendDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static string[,] mostrarMapa()
    {
        string bootstrapServ = "";
        string nombreTopic = "";
        //En caso de que no se introduzcan valores como argumento, cogemos la configuracion por defecto:
        
        Console.Write("Por favor, introduce el bootstrap Server: ");
        bootstrapServ = Console.ReadLine();
        Console.Write("Introduzca el nombre del topic: ");
        nombreTopic = Console.ReadLine();
        string[,] map = new string[20, 20];

        var config = new ConsumerConfig
        {

            BootstrapServers = bootstrapServ,
            GroupId = "console-consumer-43044"

        };

        using (var consumer = new ConsumerBuilder<Null, string[,]>(config)
        .Build())
        {
            consumer.Subscribe(nombreTopic);
            
            ConsumeResult<Null, string[,]> consumeResult = consumer.Consume();
            map = consumeResult.Value;
            
        }
        return map;
    }


    public static int Main(String[] args)
    {

        int respuesta = 0;

        do
        {
            Console.WriteLine("Selecciona una opcion:\n" +
            "1.Crear perfil\n" +
            "2.Editar perfil\n" +
            "3.Entrar al parque\n" +
            "4.Salir del parque\n" +
            "5.Salir\n");

            respuesta = Convert.ToInt16(Console.ReadLine());

            switch (respuesta)
            {
                case 1:
                    StartClient(respuesta);
                    break;
                case 2:
                    StartClient(respuesta);
                    break;
                case 3:
                    string bootstrapServ = "";
                    string nombreTopic = "";
                    //En caso de que no se introduzcan valores como argumento, cogemos la configuracion por defecto:

                    Console.Write("Por favor, introduce el bootstrap Server: ");
                    bootstrapServ = Console.ReadLine();
                    Console.Write("Introduzca el nombre del topic: ");
                    nombreTopic = Console.ReadLine();
                    string[,] map = new string[20, 20];

                    var config = new ConsumerConfig
                    {

                        BootstrapServers = bootstrapServ,
                        GroupId = "console-consumer-43044"

                    };
                   
                    using (var consumer = new ConsumerBuilder<Null, string>(config)
                    .Build())
                    {
                        consumer.Subscribe(nombreTopic);
                        for(int i = 0; i < 20; i++)
                        {
                            for(int j = 0; j < 20; j++)
                            {
                                ConsumeResult<Null, string> consumeResult = consumer.Consume();
                                map[i, j] = consumeResult.Value;
                            }
                        }
                    }
                    for (int i = 0; i < 20; i++)
                    {
                        for (int j = 0; j < 20; j++)
                        {
                            Console.Write(map[i, j]);
                        }
                        Console.WriteLine("");
                    }

                    break;
                case 4:

                    break;
                case 5:
                    break;
                default:
                    Console.WriteLine("ERROR: no se ha introducido una de las opciones disponibles");
                    break;
            }
        } while (respuesta != 5);

        return 0;
    }
}