using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectList
    {
        public int count;
        public List<Project> projects;
        public ProjectList() { projects = new List<Project>(); count = 0; }
        public void Add(Project p) { projects.Add(p); count++; }
    }
}