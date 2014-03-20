// This library uses sli-redis for redis communication

using System;

namespace DispatcherLibrary
{
    using System.Collections.Generic;
    using System.Linq;
    using sli_redis;

    public class CacheRedis : IServerAvailable
    {
        public string Host { get; private set; }

        public int Port { get; private set; }

        public RedisClient Client { get; set; }

        public CacheRedis(string host, int port)
        {
            Host = host;
            Port = port;
            Client = new RedisClient(Host, port);
        }

        public bool AskAvailable()
        {
            return true;
        }

        public List<Pool> GetPools()
        {
            var poolKeys = Client.Key.GetAll("Pool*");

            return (from poolKey in poolKeys
                    select Client.Hash.GetAll(poolKey)
                        into result
                        let id = int.Parse(result["Id"])
                        let name = result["Name"]
                        select new Pool(id, name, this)).ToList();
        }

        public List<Worker> GetWorkersFromPool(Pool pool)
        {
            string hashId = pool.ToString();

            var workerKeys = Client.Key.GetAll("Worker*");

            return (from workerKey in workerKeys
                    select Client.Hash.GetAll(workerKey)
                        into result
                        where result["PoolHashId"] == hashId
                        let id = int.Parse(result["Id"])
                        let name = result["Name"]
                        let maxJobs = int.Parse(result["MaxJobs"])
                        let currentJobs = int.Parse(result["CurrentJobs"])
                        select new Worker(id, name, maxJobs, currentJobs, this)).ToList();
        }

        public List<Worker> GetWorkers()
        {
            var workerKeys = Client.Key.GetAll("Worker*");

            return (from workerKey in workerKeys
                    select Client.Hash.GetAll(workerKey)
                        into result
                        let id = int.Parse(result["Id"])
                        let name = result["Name"]
                        let maxJobs = int.Parse(result["MaxJobs"])
                        let currentJobs = int.Parse(result["CurrentJobs"])
                        select new Worker(id, name, maxJobs, currentJobs, this)).ToList();
        }

        public void AddPool(Pool pool)
        {
            string hashId = pool.ToString();
            Client.Hash.SetIfNotExist(hashId, "Id", pool.Id.ToString());
            Client.Hash.SetIfNotExist(hashId, "Name", pool.Name);
        }

        public void AddWorkerToPool(Worker worker, Pool pool)
        {
            string hashId = worker.ToString();
            Client.Hash.SetIfNotExist(hashId, "Id", worker.Id.ToString());
            Client.Hash.SetIfNotExist(hashId, "Name", worker.Name);
            Client.Hash.SetIfNotExist(hashId, "MaxJobs", worker.MaxJobs.ToString());
            Client.Hash.SetIfNotExist(hashId, "CurrentJobs", worker.CurrentJobs.ToString());
            Client.Hash.SetIfNotExist(hashId, "PoolHashId", pool.ToString());
        }

        public void UpdateWorker(Worker worker)
        {
            string hashId = worker.ToString();
            Client.Hash.Set(hashId, "Id", worker.Id.ToString());
            Client.Hash.Set(hashId, "Name", worker.Name);
            Client.Hash.Set(hashId, "CurrentJobs", worker.CurrentJobs.ToString());

        }

        public Worker GetWorker(string workerId)
        {
            Dictionary<string, string> workerDic = Client.Hash.GetAll(workerId);
            return new Worker(int.Parse(workerDic["Id"]), workerDic["Name"], int.Parse(workerDic["MaxJobs"]), int.Parse(workerDic["CurrentJobs"]), this);
        }

        public Job GetJob(string jobId)
        {
            Dictionary<string, string> jobDic = Client.Hash.GetAll(jobId);
            return new Job(int.Parse(jobDic["Id"]), jobDic["Name"], TimeSpan.Parse(jobDic["ExecuteTime"]), (JobState)Enum.Parse(typeof(JobState), jobDic["State"]));
        }

        //public void RemoveWorkerFromPool(Worker worker)
        //{
        //    Client.Remove(worker.ToString());
        //}

        public void EmptyRedis()
        {
            //Client.GetAllKeys().ForEach(key => Client.Remove(key));
        }

        public void SaveStateWhenError()
        {
            throw new System.NotImplementedException();
        }

        public void RestoreStateAfterError()
        {
            throw new System.NotImplementedException();
        }
    }
}
