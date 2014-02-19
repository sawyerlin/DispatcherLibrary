using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Dispatcher
{
    using System.Collections.Generic;
    using DispatcherLibrary;

    class Program
    {
        private const string ProcuderName = "jobQueue";
        private readonly List<Pool> _pools;
        private readonly CacheRedis _cache;
        private const int Limit = 4;

        static void Main(string[] args)
        {
            Program program = new Program();
            for (int i = 0; i < Limit; i++)
            {
                Thread thread = new Thread(program.Run) { Name = "Client " + i };
                thread.Start();
            }
        }

        public Program()
        {
            _cache = new CacheRedis("192.168.102.49", 6379);
            _pools = new List<Pool>();
            if (_cache.AskAvailable())
                Initialise(_cache);
        }

        private void Initialise(CacheRedis cache)
        {
            Pool pool1 = new Pool(1, "Pool1", cache);
            Pool pool2 = new Pool(2, "Pool2", cache);
            _pools.Add(pool1);
            _pools.Add(pool2);

            Worker worker1 = new Worker(1, "Worker1", 10);
            Worker worker2 = new Worker(2, "Worker2", 15);
            Worker worker3 = new Worker(3, "Worker3", 20);
            pool1.AddWorker(worker1);
            pool1.AddWorker(worker2);
            pool1.AddWorker(worker3);

            Worker worker4 = new Worker(4, "Worker4", 22);
            Worker worker5 = new Worker(5, "Worker5", 30);
            pool2.AddWorker(worker4);
            pool2.AddWorker(worker5);
        }


        public void Run()
        {
            //Dispatcher dispatcher =
            //    new Dispatcher(_pools, _cache, ProcuderName);

            //while (true)
            //    dispatcher.Dispatch();



            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
           {
               NoDelay = true,
               SendTimeout = 10000
           };
            BufferedStream bufferedStream = null;

            int index = 0;
            while (true)
            {
                if (Thread.CurrentThread.Name == "Client 3")
                {

                }
                if (ConnectSocket(socket, ref bufferedStream))
                {
                    try
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes("My first Command " + index + " was sent by [" + Thread.CurrentThread.Name + "]\r\n");
                        socket.Send(bytes);
                        index++;
                        Thread.Sleep(2000);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                }
            }
        }

        private bool ConnectSocket(Socket socket, ref BufferedStream stream)
        {
            if (!socket.Connected)
            {
                socket.Connect("192.168.102.118", 8010);
                if (!socket.Connected)
                {
                    socket.Close();
                    socket = null;
                    return false;
                }

                stream = new BufferedStream(new NetworkStream(socket), 16 * 1024);
                return true;
            }
            return true;
        }
    }
}