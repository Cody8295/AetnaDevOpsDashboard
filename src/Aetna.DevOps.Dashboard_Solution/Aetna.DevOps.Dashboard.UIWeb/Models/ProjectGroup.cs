using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectGroup : OctopusModel<ProjectGroup>
    {
        public string GroupName;
        public string GroupId;
        public List<Project> Projects;

        public ProjectGroup (string groupName, string groupId)
        {
            GroupName = groupName;
            GroupId = groupId;
            Projects = new List<Project>();
        }

        public void AddProject(Project project)
        {
            Projects.Add(project);
        }

        public bool Equals(ProjectGroup other)
        {
            return (GroupName == other.GroupName && GroupId == other.GroupId && Projects.DeepEquals<Project>(other.Projects));
        }
    }
}