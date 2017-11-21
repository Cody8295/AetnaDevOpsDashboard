using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectGroupDictionary
    {
        public Dictionary<string, ProjectGroup> pGroupDictionary;
        public ProjectGroupDictionary()
        {
            pGroupDictionary = new Dictionary<string, ProjectGroup>();
        }
        public void AddProjectGroup (string groupId, ProjectGroup pGroup)
        {
            pGroupDictionary.Add(groupId, pGroup);
        }

        public void addProject (string groupId, Project project)
        {
            pGroupDictionary[groupId].AddProject(project);
        }

        public List<ProjectGroup> getProjectGroups()
        {
            return new List<ProjectGroup>(pGroupDictionary.Values);
        }
    }
}