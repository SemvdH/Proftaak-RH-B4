using System;
using System.Collections.Generic;
using System.Net.Sockets;
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
                    this.mDoctor.sendMessage(DataParser.getNewConnectionJson(client.username));
                    client.sendMessage(DataParser.getNewConnectionJson(this.mDoctor.username));
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
            Doctor.sendMessage(DataParser.getDisconnectJson(client.username));
        }

        public void NewLogin(Client client)
        {
            this.clients.Add(client);
            if (this.Doctor != null)
            {
                Doctor.sendMessage(DataParser.getNewConnectionJson(client.username));
                client.sendMessage(DataParser.getNewConnectionJson(Doctor.username));
            }
        }

        public void LogOff(Client client)
        {
            if (this.Doctor == client)
            {
                this.clients.ForEach((client) =>
                {
                    client.sendMessage(DataParser.getDisconnectJson(this.mDoctor.username));
                });
                this.Doctor = null;
            }
            this.clients.Remove(client);
        }

        public void StartSessionUser(string user)
        {
            foreach (Client client in clients)
            {
                if (client.username == user)
                {
                    client.sendMessage(DataParser.getStartSessionJson(user));
                    client.StartSession();
                }
            }
        }

        public void StopSessionUser(string user)
        {
            foreach (Client client in clients)
            {
                if (client.username == user)
                {
                    client.sendMessage(DataParser.getStopSessionJson(user));
                    client.StopSession();
                }
            }

        }
    }
}
