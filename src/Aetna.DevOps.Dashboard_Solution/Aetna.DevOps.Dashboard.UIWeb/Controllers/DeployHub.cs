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
                string projectGroups = currentState.UpdateProjectGroups();
                string projects = currentState.UpdateProjects();
                string lifecycles = currentState.UpdateLifecycles();
                string environments = currentState.UpdateEnvironments();
                string deployEvents = currentState.UpdateDeployEvents();
                string deploys = currentState.UpdateDeploys();
                string liveDeploys = currentState.UpdateLiveDeploys();

                if (projectGroups != "noChange" || projects != "noChange" || lifecycles != "noChange" || environments != "noChange" 
                                                || deployEvents != "noChange" || deploys != "noChange" || liveDeploys != "noChange")
                {
                    Clients.All.onChange(projectGroups, projects, lifecycles, environments, deployEvents, deploys, liveDeploys);
                }
            };
            timer.Enabled = true;
            timer.Start();
        }
    }
}