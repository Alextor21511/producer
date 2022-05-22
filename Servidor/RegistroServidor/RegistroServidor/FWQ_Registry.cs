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

public class AsynchronousSocketListener
{
    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public AsynchronousSocketListener()
    {
    }

    public static void StartListening()
    {
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(100);

            while (true)
            {
                // Set the event to nonsignaled state.  
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                Console.WriteLine("Waiting for a connection...");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);

                // Wait until a connection is made before continuing.  
                allDone.WaitOne();
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        allDone.Set();

        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);

        listener.BeginAccept(
    new AsyncCallback(AcceptCallback),
    listener);
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {
                // All the data has been read from the
                // client. Display it on the console.  
                Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);
                // Echo the data back to the client.  
                Send(handler, content);
            }
            else
            {
                // Not all data received. Get more.  
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }

            string[] datos = content.Split(':');
            string nombre = "";
            string alias = "";
            string password = "";

            //ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
            int orden = Int32.Parse(datos[3]);
            if (orden==1)
            {
                    SQLiteConnection cadenaConexion = new SQLiteConnection("Data Source = C:/Users/aleja/Onedrive/Escritorio/general.sqlite");
                    cadenaConexion.Open();
                    string consulta = "select * from usuarios";
                    SQLiteCommand comando = new SQLiteCommand(consulta, cadenaConexion);
                    SQLiteDataReader datos_tabla = comando.ExecuteReader();

                    bool existe_nombre = false;
                    while (datos_tabla.Read())
                    {
                        if (datos_tabla[0]==datos[0])
                        {
                            Console.WriteLine(datos[0] + datos[1] + datos[2]);

                            existe_nombre = true;
                            //Send(handler, "El nombre de usuario ya existe");
                        }
                    }
                if (existe_nombre == false)
                {
                    SQLiteCommand comando2 = new SQLiteCommand("insert into usuarios values (\"" + datos[0] + "\",\"" + datos[1] + "\",\"" + datos[2] + "\")", cadenaConexion);
                    comando2.ExecuteNonQuery();
                }
                cadenaConexion.Close();
                }
            if (orden==2)
            {
                SQLiteConnection cadenaConexion = new SQLiteConnection("Data Source = C:/Users/aleja/Onedrive/Escritorio/general.sqlite");
                cadenaConexion.Open();
                string consulta = "select * from usuarios";
                SQLiteCommand comando = new SQLiteCommand(consulta, cadenaConexion);
                SQLiteDataReader datos_tabla = comando.ExecuteReader();

                bool existe_nombre = false;
                while (datos_tabla.Read())
                {
                    if (datos_tabla[0] == datos[0] && datos_tabla[1] == datos[1] && datos_tabla[2] == datos[2])
                    {
                        Console.WriteLine(datos[0] + datos[1] + datos[2]);
                        SQLiteCommand comando2 = new SQLiteCommand("update usuarios set nombre=\"" + datos[0] + "\",alias=\"" + datos[1] + "\",password=\"" + datos[2] + "\"where nombre=\"" + datos[0] + "\"", cadenaConexion);
                        comando2.ExecuteNonQuery();
                        existe_nombre = true;
                    }
                }
                if (existe_nombre == false)
                {
                    //Send(handler, "El nombre de usuario ya existe");
                }
                cadenaConexion.Close();
            }
        }
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Main(String[] args)
    {
        StartListening();
        return 0;
    }
}