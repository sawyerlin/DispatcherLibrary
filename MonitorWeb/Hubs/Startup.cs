using Owin;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(MonitorWeb.Hubs.Startup))]
namespace MonitorWeb.Hubs
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}