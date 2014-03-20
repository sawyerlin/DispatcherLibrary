using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using DispatcherLibrary;

namespace DispatcherConsumer
{
    internal class Server
    {
        private volatile bool _shouldStop;
        private readonly TcpListener _listener;
        private const int Limit = 5;
        private readonly CacheRedis _cache;

        internal Server()
        {
            _cache = new CacheRedis("192.168.102.49", 6379);
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
                    string value = reader.ReadLine();
                    if (value == null) return;
                    Match match = Regex.Match(value, @"(?'command'\w*)\s(?'param1'[^\s]*)\s(?'param2'[^\s]*)", RegexOptions.IgnoreCase);
                    string command = match.Groups["command"].ToString();
                    string workerId = match.Groups["param1"].ToString();
                    string jobId = match.Groups["param2"].ToString();
                    if (command.Equals("WORK"))
                    {
                        Worker worker = _cache.GetWorker(workerId);
                        worker.Work(jobId);
                    }
                    string line = "[" + Thread.CurrentThread.Name + "] " + value;
                    Console.WriteLine(line);
                }
            }
        }
    }
}