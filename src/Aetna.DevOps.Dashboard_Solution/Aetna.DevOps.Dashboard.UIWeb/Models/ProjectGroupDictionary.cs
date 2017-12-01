using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectGroupDictionary : Clonable<ProjectGroupDictionary>
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

        public ProjectGroupDictionary Clone()
        {
            ProjectGroupDictionary newProjectGroupDictionary = new ProjectGroupDictionary();
            foreach (KeyValuePair<string, ProjectGroup> entry in ProjectGroups) {
                newProjectGroupDictionary.AddProjectGroup(entry.Key, entry.Value.Clone());
            }

            return newProjectGroupDictionary;
        }
    }
}