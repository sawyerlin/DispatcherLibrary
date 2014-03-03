using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DispatcherLibrary;
using HeartBeatLibrary;

namespace Dispatcher
{
    internal class CustomDispatcher : DispatcherLibrary.Dispatcher
    {
        private readonly HeartBeat _heartBeat;

        public CustomDispatcher(List<Pool> pools, CacheRedis cache, string producername)
            : base(pools, cache, producername)
        {
            List<Remote> neighbours = new List<Remote>
                {
                    new Remote("192.168.102.118", 8013)
                };

            _heartBeat = new HeartBeat(neighbours, "192.168.102.118", 8012, 0, 10);

        }

        #region Socket Trigger

        public void Run()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                SendTimeout = 10000
            };
            BufferedStream bufferedStream = null;

            int index = 0;
            while (true)
            {
                if (_heartBeat.IsMaster && ConnectSocket(socket, ref bufferedStream))
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

        private bool ConnectSocket(Socket socket,
            ref BufferedStream stream)
        {
            if (!socket.Connected)
            {
                socket.Connect("192.168.102.118", 8010);
                if (!socket.Connected)
                {
                    socket.Close();
                    return false;
                }

                stream = new BufferedStream(new NetworkStream(socket), 16 * 1024);
                return true;
            }
            return true;
        }

        #endregion

        public override void Dispatch()
        {
            while (true)
            {
                if (_heartBeat.IsMaster)
                {
                    Job job = GetJob();
                    if (job != null)
                        Console.WriteLine(job + " from dispatcher 8012");
                    else
                        Console.WriteLine("Job is null from dispatcher 8012");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}