using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Project : OctopusModel<Project>
    {
        public string GroupId;
        public string Name;
        public string Lifecycle;
        public string DeploymentProcess;
        public string Id;

        public Project(string id, string groupId, string name, string lifecycle, string deploymentProcess)
        {
            Id = id;
            GroupId = groupId;
            Name = name;
            Lifecycle = lifecycle;
            DeploymentProcess = deploymentProcess;
        }
        public string GetGroupId() { return GroupId; }

        public bool Equals(Project other)
        {
            return (GroupId == other.GroupId && Name == other.Name && Lifecycle == other.Lifecycle && DeploymentProcess == other.DeploymentProcess && Id == other.Id);
        }
    }
}