using System;
using DispatcherLibrary;

namespace DispatcherCreator
{
    class Program
    {
        private readonly CacheRedis _cache;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        public Program()
        {
            _cache = new CacheRedis("192.168.102.49", 6379);
        }

        public void Run()
        {
            if (_cache.AskAvailable())
            {
                Producer producer = new Producer(_cache, "jobQueue");
                int index = 0;
                while (true)
                {
                    Job job = new Job(index, "Job " + index, TimeSpan.FromSeconds(10), JobState.UnKnown);
                    producer.Produce(job);
                    index++;
                }
            }
        }
    }
}