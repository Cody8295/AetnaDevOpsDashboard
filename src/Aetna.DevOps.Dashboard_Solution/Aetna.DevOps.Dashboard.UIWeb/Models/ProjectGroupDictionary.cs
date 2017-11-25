using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectGroupDictionary
    {
        public Dictionary<string, ProjectGroup> ProjectGroups;
        public ProjectGroupDictionary()
        {
            ProjectGroups = new Dictionary<string, ProjectGroup>();
        }
        public void AddProjectGroup (string groupId, ProjectGroup pGroup)
        {
            ProjectGroups.Add(groupId, pGroup);
        }

        public void addProject (string groupId, Project project)
        {
            ProjectGroups[groupId].AddProject(project);
        }

        public List<ProjectGroup> getProjectGroups()
        {
            return new List<ProjectGroup>(ProjectGroups.Values);
        }
    }
}