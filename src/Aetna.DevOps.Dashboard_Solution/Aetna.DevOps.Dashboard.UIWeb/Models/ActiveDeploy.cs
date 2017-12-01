namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ActiveDeploy : Clonable<ActiveDeploy>
    {
        public string Id, ProjectId, ReleaseId, TaskId, ChannelId, ReleaseVersion, Created, QueueTime, CompletedTime, State,
            HasWarningsOrErrors, ErrorMessage, Duration, IsCurrent, IsCompleted, ProjectName, WebUrl;

        public ActiveDeploy(string id, string projectId, string releaseId, string taskId, string channelId, string releaseVersion,
            string created, string queueTime, string completedTime, string state, string hasWarningsOrErrors, string errorMessage,
            string duration, string isCurrent, string isCompleted, string projectName, string webUrl)
        {
            Id = id; ProjectId = projectId; TaskId = taskId; ReleaseId = releaseId; ChannelId = channelId; ReleaseVersion = releaseVersion;
            Created = created; QueueTime = queueTime; CompletedTime = completedTime; State = state; HasWarningsOrErrors = hasWarningsOrErrors;
            ErrorMessage = errorMessage; Duration = duration; IsCurrent = isCurrent; IsCompleted = isCompleted; ProjectName = projectName; WebUrl = webUrl;
        }

        public ActiveDeploy Clone()
        {
            return new ActiveDeploy(Id,ProjectId,ReleaseId,TaskId,ChannelId,ReleaseVersion,Created,QueueTime,CompletedTime,State,
                                    HasWarningsOrErrors,ErrorMessage,Duration,IsCurrent,IsCompleted,ProjectName,WebUrl);
        }

        public bool Equals(ActiveDeploy other)
        {
            return (Id == other.Id && ProjectId == other.ProjectId && ReleaseId == other.ReleaseId && TaskId == other.TaskId && ChannelId == other.ChannelId
                && ReleaseVersion == other.ReleaseVersion && Created == other.Created && QueueTime == other.QueueTime && CompletedTime == other.CompletedTime
                && State == other.State && HasWarningsOrErrors == other.HasWarningsOrErrors && ErrorMessage == other.ErrorMessage && Duration == other.Duration
                && IsCurrent == other.IsCurrent && IsCompleted == other.IsCompleted && ProjectName == other.ProjectName && WebUrl == other.WebUrl);
        }
    }
}