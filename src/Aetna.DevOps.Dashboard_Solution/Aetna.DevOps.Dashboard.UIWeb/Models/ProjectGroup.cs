namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectGroup
    {
        public string GroupName;
        public string GroupId;
        public ProjectList ProjectList;

        public ProjectGroup (string groupName, string groupId)
        {
            GroupName = groupName;
            GroupId = groupId;
            ProjectList = new ProjectList();
        }

        public void AddProject(Project project)
        {
            ProjectList.Add(project);
        }
    }
}