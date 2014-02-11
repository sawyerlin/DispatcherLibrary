using System.Collections.Generic;
using DispatcherLibrary;

namespace DispatcherConsumer
{
    class Program
    {
        private readonly CacheRedis _cache;
        private List<Worker> _workers;

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run();
        }

        public Program()
        {
            _cache = new CacheRedis("192.168.102.49", 6379);
            _workers = new List<Worker>();
        }

        private void Initialise(CacheRedis cache)
        {
            _workers = cache.GetWorkers();
            _workers.ForEach(worker =>
                {
                    worker.WorkerChanged += w => _cache.UpdateWorker(w);
                });
        }

        public void Run()
        {
            while (true)
            {
                if (_cache.AskAvailable())
                    Initialise(_cache);
                foreach (Worker worker in _workers)
                {
                    worker.WorkJob();
                }
            }
        }
    }
}
