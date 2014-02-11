namespace DispatcherLibrary
{
    using System.Linq;

    public class Pool
    {
        public int Id { get; private set; }

        public string Name { get; private set; }

        public CacheRedis Cache { get; private set; }

        public Pool(int id, string name, CacheRedis cache)
        {
            Id = id;
            Name = name;
            Cache = cache;

            Cache.AddPool(this);
        }

        public void AddWorker(Worker worker)
        {
            Cache.AddWorkerToPool(worker, this);
        }

        public virtual int GetJobsOfAvailableWorkers()
        {
            int jobs = -1;

            var availableWorkers = Cache.GetWorkersFromPool(this).Where(worker => worker.AskAvailable()).ToArray();
            jobs = availableWorkers.Aggregate(jobs, (current, next) => current + next.CurrentJobs);

            return jobs;
        }

        public virtual Worker GetBestAvailableWorker()
        {
            var availableWorkers = Cache.GetWorkersFromPool(this).Where(worker => worker.AskAvailable()).ToArray();
            Worker bestWorker = null;
            if (availableWorkers.Any())
                bestWorker =
                   availableWorkers
                   .OrderBy(worker => worker.CurrentJobs).First();
            return bestWorker;
        }

        public virtual void UpdateWorker(Worker worker)
        {
            Cache.UpdateWorker(worker);
        }

        public override string ToString()
        {
            return "Pool-" + Id;
        }
    }
}