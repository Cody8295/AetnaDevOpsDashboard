using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Project : Clonable<Project>
    {
        public string GroupId;
        public string Name;
        public string Lifecycle;
        public string DeploymentProcess;
        public List<string> RelatedDocs;
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

        public Project Clone()
        {
            return new Project(Id, GroupId, Name, Lifecycle, DeploymentProcess);
        }
    }
}