using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Aetna.DevOps.Dashboard.UIWeb.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
    [HubName("deployHub")]
    public class DeployHub : Hub
    {
        private static DataState currentState = DataState.Instance;
        private static System.Timers.Timer timer = new System.Timers.Timer(5000); // Set Timer to run every 5 seconds
        public DeployHub() : base()
        {
            timer.Elapsed += (sender, e) =>
            {
                if (currentState.Update())
                {
                    string serialization = currentState.JsonSerialization;
                    Clients.All.onChange(serialization);
                }
            };
            timer.Enabled = true;
            timer.Start();
        }
    }
}