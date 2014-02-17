using System.Threading;
using System.Xml;

namespace DispatcherLibrary
{
    using System;
    using System.IO;
    using System.Xml.Linq;
    using System.Configuration;

    public class Worker : EventArgs, IServerAvailable
    {
        private readonly CacheRedis _cache;

        public event Action<Worker> WorkerChanged;

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

            Console.WriteLine("The worker " + Name + string.Format(" (Current: {0}, Max: {1})", CurrentJobs, MaxJobs) + " is working on " + job.Name);

            string keyJob = job.ToString();
            _cache.Client.Hash.Set(keyJob, "Id", job.Id.ToString());
            _cache.Client.Hash.Set(keyJob, "Name", job.Name);
            _cache.Client.Hash.Set(keyJob, "ExecuteTime", job.ExecuteTime.ToString());
            _cache.Client.Hash.Set(keyJob, "State", job.State.ToString());
            _cache.Client.Hash.Set(keyJob, "Worker", ToString());

            return this;
        }

        public virtual void Update()
        {
            _cache.UpdateWorker(this);
        }

        public virtual Worker WorkJob()
        {
            string[] files = Directory.GetFiles(ConfigurationManager.AppSettings["JobPath"], "*" + ToString() + ".xml");

            foreach (string file in files)
            {

                if (WorkingJobs < CurrentJobs)
                {
                    string file1 = file + ".ongoing";
                    try
                    {
                        File.Move(file, file1);
                        Thread thread = new Thread(() => Work(file1));
                        WorkingJobs++;
                        thread.Start();
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("[Exception] The file \"" + Path.GetFileName(file) + "\" is opened by another process.");
                    }
                }
            }

            return this;
        }

        public virtual void Work(string file)
        {
            XElement element = XElement.Load(file);
            XElement elementId = element.Element("Id");
            XElement elementExecutionTime = element.Element("ExecutionTime");
            XElement elementState = element.Element("State");

            string id = elementId != null ? elementId.Value : "NULL";
            TimeSpan executionTime = elementExecutionTime != null ? XmlConvert.ToTimeSpan(elementExecutionTime.Value) : new TimeSpan(0);

            Thread.Sleep(executionTime);

            string file1 = file.Remove(file.IndexOf(".ongoing", StringComparison.Ordinal));
            if (elementState != null)
            {
                elementState.SetValue(JobState.Finished);
                element.Save(file1 + ".success");
                Console.WriteLine("Job " + id + " is finished by " + Name);
                File.Delete(file);
                CurrentJobs--;
                if (WorkerChanged != null)
                    WorkerChanged(this);
            }
            else
                File.Move(file, file1);

            WorkingJobs--;
        }

        public override string ToString()
        {
            return "Worker-" + Id;
        }
    }
}