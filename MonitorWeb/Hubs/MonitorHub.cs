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
            int index = 1;
            while (true)
            {
                Clients.All.updateWorkers(GetWorkers());
                Clients.All.updateJobs(GetJobs());
                index++;
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
    }
}