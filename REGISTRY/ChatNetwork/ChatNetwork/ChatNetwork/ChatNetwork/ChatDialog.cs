using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net; 
using System.Threading; 

namespace ChatNetwork
{

    public partial class ChatDialog : Form
    {
        private TcpClient client;
        private NetworkStream clientStream;
        public delegate void SetTextCallback(string s);
        private Form1 owner;

        public TcpClient connectedClient {
            get { return client; }
            set { client = value; }

        }

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ChatDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor which accepts Client TCP object
        /// </summary>
        /// <param name="tcpClient"></param>
        public ChatDialog(Form1 parent,  TcpClient tcpClient)
        {
            InitializeComponent();

            this.owner = parent; 
            // Get Stream Object
            connectedClient = tcpClient;
            clientStream = tcpClient.GetStream();

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = connectedClient.Client;

            //Call Asynchronous Receive Function
            connectedClient.Client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(OnReceive), state);

            //connectedClient.Client.BeginDisconnect(true, new AsyncCallback(DisconnectCallback), state);
            rtbChat.AppendText("Chat Log Here------>");
        }

        #endregion
        
        /// <summary>
        /// This function is used to display data in Rich Text Box
        /// </summary>
        /// <param name="text"></param>
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.rtbChat.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.rtbChat.SelectionColor = Color.Blue; 
                this.rtbChat.SelectedText = "\nFriend: "+text;
            }
        }

        #region Send/Receive Data From Scokets
        /// <summary>
        /// Function to Send Data to Client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] bt;
            bt = Encoding.ASCII.GetBytes(txtMessage.Text);
            connectedClient.Client.Send(bt);

            rtbChat.SelectionColor = Color.IndianRed; 
            rtbChat.SelectedText = "\nMe:     " + txtMessage.Text;
            txtMessage.Text = ""; 
        }

        /// <summary>
        /// Asynchronous Callback function which receives data from Server
        /// </summary>
        /// <param name="ar"></param>
        public void OnReceive(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead;

            if (handler.Connected)
            {

                // Read data from the client socket. 
                try
                {
                    bytesRead = handler.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        // There  might be more data, so store the data received so far.
                        state.sb.Remove(0, state.sb.Length);
                        state.sb.Append(Encoding.ASCII.GetString(
                                         state.buffer, 0, bytesRead));

                        // Display Text in Rich Text Box
                        content = state.sb.ToString();
                        SetText(content);

                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(OnReceive), state);

                    }
                }

                catch (SocketException socketException)
                {
                    //WSAECONNRESET, the other side closed impolitely
                    if (socketException.ErrorCode == 10054 || ((socketException.ErrorCode != 10004) && (socketException.ErrorCode != 10053)))
                    {
                        // Complete the disconnect request.
                        String remoteIP = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
                        String remotePort = ((IPEndPoint)handler.RemoteEndPoint).Port.ToString();
                        this.owner.DisconnectClient(remoteIP, remotePort);

                        handler.Close();
                        handler = null;

                    }
                }

            // Eat up exception....Hmmmm I'm loving eat!!!
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message + "\n" + exception.StackTrace);
                    
                }
            }
        }

        #endregion

        #region Menu Commands
        /// <summary>
        /// Exit Event Handler. Exit menu item is selected, the dialog box is hidden from user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide(); 
        }
        
        /// <summary>
        /// Event Called when Chat Dialog Box is Closed by user. Same as exit event of Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatDialog_FormClosing(Object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;  
            this.Hide();

        }
        #endregion
    }


    #region StateObject Class Definition
    /// <summary>
    /// StateObject Class to read data from Client
    /// </summary>
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
    #endregion
}