namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ProjectGroup
    {
        public string groupName;
        public string groupId;
        public ProjectList projectList;

        public ProjectGroup (string groupName, string groupId)
        {
            this.groupName = groupName;
            this.groupId = groupId;
            projectList = new ProjectList();
        }

        public void AddProject(Project project)
        {
            projectList.Add(project);
        }
    }
}