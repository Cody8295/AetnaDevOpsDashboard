using System;
using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class LiveDeploy : OctopusModel<LiveDeploy>
    {
        public string Id;
        public string ProjectId;
        public string ReleaseId;
        public string WebUrl;
        public string EnvironmentId;
        public string CreationTime;


        public LiveDeploy(string id, string projectId, string releaseId, string environmentId, string webUrl, string creationTime)
        {
            Id = id;
            ProjectId = projectId;
            ReleaseId = releaseId;
            EnvironmentId = environmentId;
            WebUrl = webUrl;
            CreationTime = creationTime;
        }
        public override string ToString()
        {
            return Id;
        }

        public bool Equals(LiveDeploy other)
        {
            return (Id==other.Id && ProjectId == other.ProjectId && ReleaseId == other.ReleaseId && WebUrl == other.WebUrl && EnvironmentId == other.EnvironmentId && CreationTime == other.CreationTime);
        }
    }
}