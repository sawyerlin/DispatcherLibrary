using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DispatcherLibrary
{
    public class Dispatcher
    {
        private readonly CacheRedis _cache;

        private readonly List<Pool> _pools;

        private readonly string _producerName;

        public Dispatcher(List<Pool> pools, CacheRedis cache, string producername)
        {
            _pools = pools;
            _cache = cache;
            _producerName = producername;
        }

        public Pool GetAvailablePool()
        {
            Pool bestPool = _pools.OrderBy(pool => pool.GetJobsOfAvailableWorkers()).First();
            return bestPool;
        }

        public virtual void Dispatch()
        {
            Job job;
            if ((job = GetJob()) != null)
            {
                Pool availablePool = GetAvailablePool();
                if (availablePool != null)
                {
                    Worker worker = availablePool.GetBestAvailableWorker();
                    if (worker != null)
                        worker.AllocatJob(job).Update();
                }
            }
        }

        protected Job GetJob()
        {
            string job = _cache.Client.List.DeQueue(_producerName);
            job = job.TrimStart('\'').TrimEnd('\'');
            return JsonConvert.DeserializeObject<Job>(job);
        }
    }
}