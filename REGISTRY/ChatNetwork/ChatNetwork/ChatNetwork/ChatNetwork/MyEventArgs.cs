using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets; 

namespace ChatNetwork
{
    class MyEventArgs:EventArgs 
    {
        private TcpClient  sock;
        public TcpClient  clientSock {
            get { return sock; }
            set { sock = value;}
         }

        public MyEventArgs(TcpClient tcpClient) {
            sock = tcpClient;
        }


    }
}
