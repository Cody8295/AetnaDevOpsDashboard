using Owin;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
    public class Startup
    {
        public static void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}