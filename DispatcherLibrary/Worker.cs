namespace DispatcherLibrary
{
    using System;
    using System.Threading;

    public class Worker : EventArgs, IServerAvailable
    {
        private readonly CacheRedis _cache;

        public int Id { get; private set; }
        public string Name { get; private set; }
        public int MaxJobs { get; private set; }
        public int CurrentJobs { get; set; }
        public int WorkingJobs { get; set; }

        public Worker(int id, string name, int maxJobs)
        {
            Id = id;
            Name = name;
            MaxJobs = maxJobs;
            CurrentJobs = 0;
        }

        public Worker(int id, string name, int maxJobs, int currentJobs, CacheRedis cache)
            : this(id, name, maxJobs)
        {
            CurrentJobs = currentJobs;
            _cache = cache;
        }

        public virtual bool AskAvailable()
        {
            return CurrentJobs < MaxJobs;
        }

        public virtual Worker AllocatJob(Job job)
        {
            CurrentJobs++;
            job.State = JobState.OnGoing;

            string keyJob = job.ToString();
            _cache.Client.Hash.Set(keyJob, "Id", job.Id.ToString());
            _cache.Client.Hash.Set(keyJob, "Name", job.Name);
            _cache.Client.Hash.Set(keyJob, "ExecuteTime", job.ExecuteTime.ToString());
            _cache.Client.Hash.Set(keyJob, "State", job.State.ToString());
            _cache.Client.Hash.Set(keyJob, "Worker", ToString());

            string keyWorker = ToString();
            _cache.Client.Hash.Set(keyWorker, "CurrentJobs", CurrentJobs.ToString());

            Console.WriteLine("The worker " + Name + string.Format(" (Current: {0}, Max: {1})", CurrentJobs, MaxJobs) + " is working on " + job.Name);

            return this;
        }

        public virtual void Update()
        {
            _cache.UpdateWorker(this);
        }

        public virtual void Work(string jobId)
        {
            Thread.Sleep(10000); // 10 seconds
            _cache.Client.Hash.Set(jobId, "State", JobState.Finished.ToString());
            if (CurrentJobs > 0)
            {
                CurrentJobs--;
                Update();
            }
        }

        public override string ToString()
        {
            return "Worker-" + Id;
        }
    }
}