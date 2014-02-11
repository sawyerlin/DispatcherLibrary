using System;
using System.Collections.Generic;
using System.Linq;
using DispatcherLibrary;

namespace DispatcherMonitor
{
    class Program
    {
        static int _origRow;
        static int _origCol;
        private static CacheRedis _cache;

        static void Main(string[] args)
        {
            _origRow = Console.CursorTop;
            _origCol = Console.CursorLeft;

            _cache = new CacheRedis("192.168.102.49", 6379);

            DateTime dateTime = DateTime.Now;
            while (true)
            {
                if (DateTime.Now - dateTime >= TimeSpan.FromMilliseconds(500))
                {
                    Refresh();
                    dateTime = DateTime.Now;
                }
            }
        }

        static void Refresh()
        {
            List<Pool> pools = _cache.GetPools();

            int row = 0;
            const int col = 2;
            string result = string.Empty;

            foreach (Pool pool in pools)
            {
                WriteLine("=", ref row, 40);
                List<Worker> workers = _cache.GetWorkersFromPool(pool);

                int currentJobs = 0;
                int maxJobs = 0;
                currentJobs = workers.Aggregate(currentJobs, (current, next) => current + next.CurrentJobs);
                maxJobs = workers.Aggregate(maxJobs, (current, next) => current + next.MaxJobs);
                result += "[" + currentJobs + "]";
                WriteAtLine(pool.Name + " (" + currentJobs + " / " + maxJobs + ")", ref row, 5);
                WriteLine("=", ref row, 40);

                foreach (Worker worker in workers)
                {
                    WriteLine("*", ref row, 38, col);
                    WriteAtLine("Id: " + worker.Id, ref row, col);
                    WriteAtLine("Name: " + worker.Name, ref row, col);
                    WriteAtLine("MaxJobs: " + worker.MaxJobs, ref row, col);
                    WriteAtLine("CurrentJobs: " + worker.CurrentJobs, ref row, col);
                    WriteLine("*", ref row, 38, col);
                }
                WriteLine("=", ref row, 40);
            }

            WriteAt(result, row, 0);
        }

        static void WriteAtLine(string s, ref int row, int col)
        {
            WriteAt(s, row, col);
            row++;
        }

        static void WriteLine(string s, ref int row, int size, int col = 0)
        {
            for (int i = 0 + col; i < size + col; i++)
                WriteAt(s, row, i);
            row++;
        }

        static void WriteAt(string s, int y, int x)
        {
            try
            {
                Console.SetCursorPosition(_origCol + x, _origRow + y);
                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
            }
        }
    }
}
