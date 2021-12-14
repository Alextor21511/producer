using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;  
using System.Net; 

namespace ChatNetwork
{
    public partial class Form1 : Form
    {
        private TcpListener tcpServer;
        private TcpClient tcpClient; 
        private Thread th;
        private ChatDialog ctd;
        private ArrayList formArray = new ArrayList();
        private ArrayList threadArray = new ArrayList();
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Changed;
        public delegate void SetListBoxItem(String str, String type); 
        
        /// <summary>
        /// Constructor.  It adds event to handle when client is connected and Initializes Tree View.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            
            // Add Event to handle when a client is connected
            Changed += new ChangedEventHandler(ClientAdded);

            // Add node in Tree View
            TreeNode node;
            node = tvClientList.Nodes.Add("Connected Clients");
            ImageList il = new ImageList();
         //   il.Images.Add(new Icon("audio.ico"));
            il.Images.Add(new Icon("messenger.ico"));
            tvClientList.ImageList = il;
            node.ImageIndex = 1;

        }

        #region Server Start/Stop

        /// <summary>
        /// This function spawns new thread for TCP communication
        /// </summary>
        public void StartServer() 
        {
            tbPortNumber.Enabled = false; 
            th = new Thread(new ThreadStart(StartListen));
            th.Start(); 
            
        }

        /// <summary>
        /// Server listens on the given port and accepts the connection from Client.
        /// As soon as the connection id made a dialog box opens up for Chatting.
        /// </summary>
        public void StartListen() {
             
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            tcpServer = new TcpListener(localAddr, Int32.Parse(tbPortNumber.Text));
            tcpServer.Start();

            // Keep on accepting Client Connection
            while (true)
            {
                
                // New Client connected, call Event to handle it.
                Thread t = new Thread(new ParameterizedThreadStart(NewClient));
                tcpClient = tcpServer.AcceptTcpClient();
                t.Start(tcpClient); 
                
            }

        }

        /// <summary>
        /// Function to stop the TCP communication. It kills the thread and closes client connection
        /// </summary>
        public void StopServer()
        {
             
             if (tcpServer != null)
             {

                 // Close all Socket connection
                 foreach (ChatDialog  c in formArray )
                 {
                     c.connectedClient.Client.Close();  
                 }

                 // Abort All Running Threads
                 foreach (Thread t in threadArray)
                 {
                     t.Abort();
                 }

                 // Clear all ArrayList
                 threadArray.Clear();
                 formArray.Clear();
                 tvClientList.Nodes[0].Nodes.Clear();  

                 // Abort Listening Thread and Stop listening
                 th.Abort();
                 tcpServer.Stop();
             }
            tbPortNumber.Enabled = true; 
        }


        /// <summary>
        /// Fuction checks whether to start or stop the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ckbServerControl_CheckedChanged(object sender, EventArgs e)
        {
            
            
            if (ckbServerControl.Checked == true)
            {
                // validate the port number
                try{
                    int port;
                    port=Int32.Parse(tbPortNumber.Text);
                    
                    StartServer();
                }
                catch(Exception ex){ 
                    MessageBox.Show("Please enter the correct port number!!!");
                    ckbServerControl.Checked = false;  
                }
            }

            else {
                StopServer();
            }
        }

        #endregion

        #region Add/remove Clients
        /// <summary>
        /// 
        /// </summary>
        public void NewClient(Object  obj) {
            ClientAdded(this, new MyEventArgs((TcpClient)obj));
        }

        /// <summary>
        /// Event Fired when a Client gets connected. Following actions are performed
        /// 1. Update Tree view
        /// 2. Open a chat box to chat with client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ClientAdded(object sender, EventArgs e) {
            tcpClient = ((MyEventArgs)e).clientSock; 
            String remoteIP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
            String remotePort = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port.ToString();

            // Call Delegate Function to update Tree View
            UpdateClientList(remoteIP + " : " + remotePort, "Add"); 

            // Show Dialog Box for Chatting
            ctd = new ChatDialog(this, tcpClient);
            ctd.Text = "Connected to " + remoteIP + "on port number " + remotePort;

            // Add Dialog Box Object to Array List
            formArray.Add(ctd);
            threadArray.Add(Thread.CurrentThread);
            ctd.ShowDialog();
              
        }


        /// <summary>
        /// Delegate Function to update List box.
        /// If type parameter is "Add", then client information is added in Tree View
        /// else the entry is delete from Tree View.
        /// </summary>
        /// <param name="str"></param>
        private void UpdateClientList(string str, string type)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.tvClientList.InvokeRequired)
            {
                SetListBoxItem d= new SetListBoxItem(UpdateClientList);
                this.Invoke(d, new object[] { str, type });
            }
            else
            {
                // If type is Add, the add Client info in Tree View
                if (type.Equals("Add"))
                {
                    this.tvClientList.Nodes[0].Nodes.Add(str);
                }
                // Else delete Client information from Tree View
                else{
                            foreach (TreeNode n in this.tvClientList.Nodes[0].Nodes)
                            {
                                if (n.Text.Equals(str))
                                        this.tvClientList.Nodes.Remove(n);
                            }
                        }
             
            }
        }

        /// <summary>
        /// Event called when Tree View Client list is double clicked. 
        /// When a node is double clicked, corresponding chat Dialog box is made visible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvClientList_DoubleClick(object sender, System.EventArgs e) {
            int index = tvClientList.SelectedNode.Index;
            
            // Open Hidden Dialog Box
            if ( !((ChatDialog)formArray[index]).Visible)
            ((ChatDialog)formArray[index]).Show(); 
        }


        public void DisconnectClient(String remoteIP, String remotePort) {
            // Delete Client from Tree View
            UpdateClientList(remoteIP + " : " + remotePort, "Delete");

            // Find Client Chat Dialog box corresponding to this Socket
            int counter = 0;
            foreach (ChatDialog c in formArray)
            {
                String remoteIP1 = ((IPEndPoint)c.connectedClient.Client.RemoteEndPoint).Address.ToString();
                String remotePort1 = ((IPEndPoint)c.connectedClient.Client.RemoteEndPoint).Port.ToString();

                if (remoteIP1.Equals(remoteIP) && remotePort1.Equals(remotePort))
                {
                    break;
                }
                counter++;

            }

            // Terminate Chat Dialog Box
            ChatDialog cd = (ChatDialog)formArray[counter];
            formArray.RemoveAt(counter);
            
            ((Thread)(threadArray[counter])).Abort();
            threadArray.RemoveAt(counter);

        }
        #endregion

        #region Menu Commands

        /// <summary>
        /// Event Called when Main Form is closed. It stops the server and Disposes all Resources
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(Object sender, FormClosingEventArgs e) {
            StopServer();
        
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new MyAboutBox()).ShowDialog(); 
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion
    }



}