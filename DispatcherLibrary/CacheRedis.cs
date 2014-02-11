using System.Collections.Generic;
using sli_redis;

// This library uses sli-redis for redis communication
namespace DispatcherLibrary
{
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
            List<Pool> pools = new List<Pool>();

            var poolKeys = Client.Key.GetAll("Pool*");

            foreach (string poolKey in poolKeys)
            {
                Dictionary<string, string> result = Client.Hash.GetAll(poolKey);
                int id = int.Parse(result["Id"]);
                string name = result["Name"];

                pools.Add(new Pool(id, name, this));
            }

            return pools;
        }

        public List<Worker> GetWorkersFromPool(Pool pool)
        {
            string hashId = pool.ToString();
            List<Worker> workers = new List<Worker>();

            var workerKeys = Client.Key.GetAll("Worker*");

            foreach (string workerKey in workerKeys)
            {
                Dictionary<string, string> result = Client.Hash.GetAll(workerKey);
                if (result["PoolHashId"] == hashId)
                {
                    int id = int.Parse(result["Id"]);
                    string name = result["Name"];
                    int maxJobs = int.Parse(result["MaxJobs"]);
                    int currentJobs = int.Parse(result["CurrentJobs"]);

                    workers.Add(new Worker(id, name, maxJobs, currentJobs));
                }
            }
            return workers;
        }

        public List<Worker> GetWorkers()
        {
            List<Worker> workers = new List<Worker>();

            var workerKeys = Client.Key.GetAll("Worker*");
            foreach (string workerKey in workerKeys)
            {
                Dictionary<string, string> result = Client.Hash.GetAll(workerKey);
                int id = int.Parse(result["Id"]);
                string name = result["Name"];
                int maxJobs = int.Parse(result["MaxJobs"]);
                int currentJobs = int.Parse(result["CurrentJobs"]);

                workers.Add(new Worker(id, name, maxJobs, currentJobs));
            }

            return workers;
        }

        public void AddPool(Pool pool)
        {
            string hashId = pool.ToString();
            Client.Hash.Set(hashId, "Id", pool.Id.ToString());
            Client.Hash.Set(hashId, "Name", pool.Name);
        }

        public void AddWorkerToPool(Worker worker, Pool pool)
        {
            string hashId = worker.ToString();
            Client.Hash.Set(hashId, "Id", worker.Id.ToString());
            Client.Hash.Set(hashId, "Name", worker.Name);
            Client.Hash.Set(hashId, "MaxJobs", worker.MaxJobs.ToString());
            Client.Hash.Set(hashId, "CurrentJobs", worker.CurrentJobs.ToString());
            Client.Hash.Set(hashId, "PoolHashId", pool.ToString());
        }

        public void UpdateWorker(Worker worker)
        {
            string hashId = worker.ToString();
            Client.Hash.Set(hashId, "Id", worker.Id.ToString());
            Client.Hash.Set(hashId, "Name", worker.Name);
            Client.Hash.Set(hashId, "CurrentJobs", worker.CurrentJobs.ToString());

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
