using System;
using System.Net.Sockets;

namespace Client
{
    class Client
    {
        private TcpClient client;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //connect fiets




        }

        public Client() : this("localhost", 5555)
        {

        }

        public Client(string adress, int port)
        {
            this.client = new TcpClient();
            client.BeginConnect(adress, 15243, new AsyncCallback(OnConnect), null);
        }
    }
}
