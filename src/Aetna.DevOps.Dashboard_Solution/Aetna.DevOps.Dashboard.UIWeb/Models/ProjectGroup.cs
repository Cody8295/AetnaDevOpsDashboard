namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectGroup
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

        public ProjectGroup Clone()
        {
            ProjectGroup newProjectGroup = new ProjectGroup(GroupName, GroupId);
            newProjectGroup.Projects = Projects.Clone();
            return newProjectGroup;
        }
    }
}