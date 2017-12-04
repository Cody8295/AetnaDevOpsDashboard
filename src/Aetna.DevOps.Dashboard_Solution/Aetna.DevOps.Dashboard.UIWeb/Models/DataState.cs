using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Aetna.DevOps.Dashboard.UIWeb.Controllers;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class DataState
    {
        public List<ProjectGroup> ProjectGroups { get; set; }
        public List<Project> Projects { get; set; }
        public int Lifecycles { get; set; } 
        public List<Environment> Environments { get; set; }
        public List<DeployEvent> DeployEvents { get; set; }
        public List<Deploy> LiveDeploys { get; set; }
        public List<Deploy> Deploys { get; set; }

        public DataState()
        {
            OctopusController.UpdateDataState(this);
        }

        public Dictionary<string, bool> IsChanged { get; set; }
    }
}