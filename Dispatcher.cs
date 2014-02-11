using System.Collections.Generic;
using System.Linq;

namespace DispatcherLibrary
{
    public class Dispatcher
    {
        private const int MaxJobs = 25;

        private readonly List<Pool> _pools;
        private readonly Queue<Job> _jobs;

        public bool IsFull { get { return _jobs.Count == MaxJobs; } }

        public Dispatcher(List<Pool> pools)
        {
            _pools = pools;
            _jobs = new Queue<Job>();
        }

        public void AddJob(Job job)
        {
            _jobs.Enqueue(job);
        }

        public Pool GetAvailablePool()
        {
            Pool bestPool = _pools.OrderBy(pool => pool.GetJobsOfAvailableWorkers()).First();
            return bestPool;
        }

        public void Dispatch()
        {
            while (_jobs.Count > 0)
            {
                Pool availablePool = GetAvailablePool();
                if (availablePool != null)
                {
                    Worker worker = availablePool.GetBestAvailableWorker();
                    if (worker != null) { 
                        worker.AllocatJob(_jobs.Dequeue());
                        availablePool.UpdateWorker(worker);
                    }
                }
            }
        }
    }
}
