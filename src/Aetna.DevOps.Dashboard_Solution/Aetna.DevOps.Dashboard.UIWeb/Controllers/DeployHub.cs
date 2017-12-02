using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Aetna.DevOps.Dashboard.UIWeb.Models;

namespace Aetna.DevOps.Dashboard.UIWeb.Controllers
{
    [HubName("deployHub")]
    public class DeployHub : Hub
    {
        private static DataState currentState = new DataState();
        private static System.Timers.Timer timer = new System.Timers.Timer(500); // Set Timer to run every .5 seconds
        public DeployHub() : base()
        {
            timer.Elapsed += (sender, e) =>
            {
                if (OctopusController.UpdateDataState(currentState))
                {
                    Clients.All.onChange(currentState.Clone());
                }
            };
            timer.Enabled = true;
            timer.Start();
        }
    }
}