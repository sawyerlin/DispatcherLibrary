namespace DispatcherConsumer
{
    class Program
    {
        private readonly Server _server;

        static void Main(string[] args)
        {
            Program program = new Program();
        }

        public Program()
        {
            _server = new Server();
            _server.Run();
        }
    }
}