using System.Collections.Generic;
using System.Threading;
using DispatcherLibrary;
using Microsoft.AspNet.SignalR;

namespace MonitorWeb.Hubs
{
    public class MonitorHub : Hub
    {
        private readonly CacheRedis _cache;

        public MonitorHub()
        {
            _cache = new CacheRedis("192.168.102.49", 6379);
        }

        public void Begin()
        {
            while (true)
            {
                Clients.All.updateWaitingJobs(GetWaitingJobs());
                Clients.All.updateWorkers(GetWorkers());
                Clients.All.updateJobs(GetJobs());
                Thread.Sleep(500);
            }
        }

        private List<Worker> GetWorkers()
        {
            List<Worker> workers = _cache.GetWorkers();
            return workers;
        }

        private List<Job> GetJobs()
        {
            List<Job> jobs = _cache.GetJobs();
            return jobs;
        }

        private List<Job> GetWaitingJobs()
        {
            List<Job> jobs = _cache.GetWaitingJobs();
            return jobs;
        }
    }
}