using System;
using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Deploy : OctopusModel<Deploy>
    {
        public enum State
        {
            Executing,
            Queued,
            Success,
            Failed,
            Canceled,
            Unknown
        }
        public string Id;
        public string ProjectId;
        public string ReleaseId;
        public string WebUrl;
        public string EnvironmentId;
        public string CreationTime;
        public State TaskState;

        


        public Deploy(string id, string projectId, string releaseId, string environmentId, string webUrl, string creationTime, State taskState)
        { 
            Id = id;
            ProjectId = projectId;
            ReleaseId = releaseId;
            EnvironmentId = environmentId;
            WebUrl = webUrl;
            CreationTime = creationTime;
            TaskState = taskState;
        }
        public override string ToString()
        {
            return Id;
        }

        public bool Equals(Deploy other)
        {
            return (Id==other.Id && ProjectId == other.ProjectId && ReleaseId == other.ReleaseId && WebUrl == other.WebUrl && EnvironmentId == other.EnvironmentId && CreationTime == other.CreationTime && TaskState == other.TaskState);
        }
    }
}