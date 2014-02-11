using System;

namespace DispatcherLibrary
{
    public class Job
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public TimeSpan ExecuteTime { get; set; }

        public JobState State { get; set; }

        public Job(int id, string name, TimeSpan executeTime, JobState state)
        {
            Id = id;
            Name = name;
            ExecuteTime = executeTime;
            State = state;
        }
    }
}
