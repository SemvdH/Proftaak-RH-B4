using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Communication communication = new Communication(new TcpListener(IPAddress.Any, 5555));
            communication.Start();

            while (true)
            {

            }
        }
    }
}
