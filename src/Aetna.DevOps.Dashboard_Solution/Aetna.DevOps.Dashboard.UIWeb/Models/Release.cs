using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Release
    {
        public string Id, Version, Projectid, Channelid, Assembled, Releasenotes, WebUrl;
        public List<ActiveDeploy> ReleaseDeploys;

        public Release(string id, string version, string projectId, string channelId, string assembled, string releaseNotes, List<ActiveDeploy> releaseDeploys, string webUrl)
        {
            Id = id;
            Version = version;
            Projectid = projectId;
            Channelid = channelId;
            Assembled = assembled;
            Releasenotes = releaseNotes;
            ReleaseDeploys = releaseDeploys;
            WebUrl = webUrl;
        }
    }
}