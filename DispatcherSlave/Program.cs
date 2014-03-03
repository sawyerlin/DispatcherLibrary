using System.Collections.Generic;
using DispatcherLibrary;

namespace DispatcherSlave
{
    class Program
    {
        private const string ProcuderName = "jobQueue";
        private readonly List<Pool> _pools;
        private readonly CacheRedis _cache;
        private readonly CustomDispatcher _dispatcher;

        static void Main(string[] args)
        {
            Program program = new Program();
        }

        public Program()
        {
            _cache = new CacheRedis("192.168.102.49", 6379);
            _pools = new List<Pool>();
            if (_cache.AskAvailable())
                Initialise(_cache);

            _dispatcher = new CustomDispatcher(_pools, _cache, ProcuderName);
            _dispatcher.Dispatch();
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
    }
}