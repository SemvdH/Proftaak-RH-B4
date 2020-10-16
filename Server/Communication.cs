using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using DoctorApp.Utils;
using Util;

namespace Server
{
    class Communication
    {
        private TcpListener listener;
        private List<Client> clients;
        private Client mDoctor;
        public Client Doctor
        {
            get
            {
                return this.mDoctor;
            }
            set
            {
                this.mDoctor = value;
                this.clients.ForEach((client) =>
                {
                    Debug.WriteLine("foreach called for " + client.username);
                    byte[] dinges = DataParser.getNewConnectionJson(client.username);
                    Debug.WriteLine("foreach " + Encoding.ASCII.GetString(dinges.Skip(5).ToArray()));
                    this.mDoctor.sendMessage(dinges);
                });
            }
        }
        public Communication(TcpListener listener)
        {
            this.listener = listener;
            this.clients = new List<Client>();
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine($"==========================================================================\n" +
                $"\tstarted accepting clients at {DateTime.Now}\n" +
                $"==========================================================================");
            listener.BeginAcceptTcpClient(new AsyncCallback(OnConnect), null);
        }

        private void OnConnect(IAsyncResult ar)
        {

            var tcpClient = listener.EndAcceptTcpClient(ar);
            Console.WriteLine($"Client connected from {tcpClient.Client.RemoteEndPoint}");
            new Client(this, tcpClient);
            listener.BeginAcceptTcpClient(new AsyncCallback(OnConnect), null);
        }

        internal void Disconnect(Client client)
        {
            clients.Remove(client);
        }

        public void NewLogin(Client client)
        {
            this.clients.Add(client);
            Debug.WriteLine("amount of clients is now " + this.clients.Count);
            var dinges = DataParser.getNewConnectionJson(client.username);
            Debug.WriteLine("new login" + Encoding.ASCII.GetString(dinges));
            Doctor?.sendMessage(dinges);
        }

        public void LogOff(Client client)
        {
            if (this.Doctor == client)
            {
                this.Doctor = null;
            }
            this.clients.Remove(client);
        }
    }
}
