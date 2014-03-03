using Newtonsoft.Json;

namespace DispatcherLibrary
{
    public class Producer
    {
        private readonly CacheRedis _client;
        private readonly string _name;

        public Producer(CacheRedis client, string name)
        {
            _client = client;
            _name = name;
        }

        public void Produce(Job job)
        {
            string json = "'" + JsonConvert.SerializeObject(job) + "'";
            _client.Client.List.EnQueue(_name, json);
        }
    }
}