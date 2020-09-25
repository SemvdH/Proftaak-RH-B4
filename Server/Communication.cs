using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Communication
    {
        private TcpListener listener;
        private List<Client> clients;

        public Communication(TcpListener listener)
        {
            this.listener = listener;
            this.clients = new List<Client>();
        }

        public void Start()
        {
            listener.Start();
            listener.BeginAcceptTcpClient(new AsyncCallback(OnConnect), null);
        }

        private void OnConnect(IAsyncResult ar)
        {
            var tcpClient = listener.EndAcceptTcpClient(ar);
            Console.WriteLine($"Client connected from {tcpClient.Client.RemoteEndPoint}");
            clients.Add(new Client(this, tcpClient));
            listener.BeginAcceptTcpClient(new AsyncCallback(OnConnect), null);
        }

        internal void Disconnect(Client client)
        {
            clients.Remove(client);
        }
    }
}
