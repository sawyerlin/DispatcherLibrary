using System;
using System.Collections.Generic;
using System.Threading;
using DispatcherLibrary;
using HeartBeatLibrary;

namespace DispatcherCreator
{
    class Program
    {
        private readonly CacheRedis _cache;
        private readonly HeartBeat _heartBeat;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        public Program()
        {
            _cache = new CacheRedis("192.168.102.49", 6379);
            List<Remote> neighbours = new List<Remote>
                {
                    new Remote("192.168.102.118", 8011)
                };

            _heartBeat = new HeartBeat(neighbours, "192.168.102.118", 8010, 0, 10);
        }

        public void Run()
        {
            if (_cache.AskAvailable())
            {
                Producer producer = new Producer(_cache, "jobQueue");
                int index = 0;
                while (true)
                {
                    Job job = new Job(index, "Job-8010-" + index, TimeSpan.FromSeconds(10), JobState.UnKnown);
                    if (_heartBeat.IsMaster)
                    {
                        producer.Produce(job);
                        Console.WriteLine("Master is 8010");
                        Thread.Sleep(1000);
                        index++;
                    }
                }
            }
        }
    }
}