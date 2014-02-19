using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DispatcherConsumer
{
    internal class Server
    {
        private volatile bool _shouldStop;
        private readonly TcpListener _listener;
        private const int Limit = 5;

        internal Server()
        {
            int port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            IPAddress address;
            if (!IPAddress.TryParse(ConfigurationManager.AppSettings["Host"], out address))
                address = Dns.GetHostAddresses(ConfigurationManager.AppSettings["Host"]).First(adr => adr.AddressFamily == AddressFamily.InterNetwork);
            _listener = new TcpListener(address, port);
        }

        internal void Run()
        {
            _listener.Start();
            for (int i = 0; i < Limit; i++)
            {
                Thread thread = new Thread(Service) { Name = "Session " + i };
                thread.Start();
            }
        }

        internal void Stop()
        {
            _shouldStop = true;
        }

        private void Service()
        {
            while (!_shouldStop)
            {
                Socket socket = _listener.AcceptSocket();
                Stream stream = new NetworkStream(socket);
                while (socket.Connected)
                {
                    StreamReader reader = new StreamReader(stream);
                    string line = "[" + Thread.CurrentThread.Name + "] " + reader.ReadLine();
                    Console.WriteLine(line);
                }
            }
        }
    }
}