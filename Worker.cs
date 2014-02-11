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

        public Worker(int id, string name, int maxJobs, int currentJobs)
            : this(id, name, maxJobs)
        {
            CurrentJobs = currentJobs;
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

            XElement jobElement = new XElement("Job",
                new XElement("Id", job.Id),
                new XElement("Name", job.Name),
                new XElement("ExecutionTime", job.ExecuteTime),
                new XElement("State", job.State),
                new XElement("Worker",
                    new XElement("Id", Id),
                    new XElement("Name", Name)
                    ));

            jobElement.Save(Path.Combine(ConfigurationManager.AppSettings["JobPath"], job.Name + "_" + ToString() + ".xml"));

            return this;
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