using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Project
    {
        public string groupId;
        public string name;
        public string lifecycle;
        public string deploymentProcess;
        public List<string> relatedDocs;
        public string id;

        public Project(string id, string groupId, string name, string lifecycle, string deploymentProcess)
        {
            this.id = id;
            this.groupId = groupId;
            this.name = name;
            this.lifecycle = lifecycle;
            this.deploymentProcess = deploymentProcess;
        }
        public string getGroupId() { return groupId; }
    }
}