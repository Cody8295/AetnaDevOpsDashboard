using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class DataState
    {
        public List<ProjectGroup> ProjectGroups { get; set; }
        public List<Project> Projects { get; set; }
        public int Lifecycles { get; set; } 
        public List<Environment> Environments { get; set; }
        public List<Deploy> Deploys { get; set; }

        public Dictionary<string, bool> IsChanged { get; set; }

        public DataState Clone()
        {
            DataState newDataState = new DataState();
            newDataState.ProjectGroups = ProjectGroups.Clone();
            newDataState.Projects = Projects.Clone();
            newDataState.Lifecycles = Lifecycles;
            newDataState.Environments = Environments.Clone();
            newDataState.Deploys = Deploys.Clone();
            newDataState.IsChanged = IsChanged.Clone();

            return newDataState;
        }
    }
}