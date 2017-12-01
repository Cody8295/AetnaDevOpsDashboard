using System;
using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class DataState
    {
        public List<ProjectGroup> ProjectGroups { get; set; }
        public List<Project> Projects { get; set; }
        public int Lifecycles { get; set; } 
        public List<Environment> Environments { get; set; }
        public List<Deploy> Deploys { get; set; }

        public Dictionary<String, Boolean> IsChanged { get; set; }
    }
}