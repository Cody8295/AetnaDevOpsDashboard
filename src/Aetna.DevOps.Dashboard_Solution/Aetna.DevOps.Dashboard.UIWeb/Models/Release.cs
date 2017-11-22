using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Release
    {
        public string id, version, projectid, channelid, assembled, releasenotes, webUrl;
        public List<ActiveDeploy> releaseDeploys;

        public Release(string Id, string Version, string ProjectId, string ChannelId, string Assembled, string ReleaseNotes, List<ActiveDeploy> ReleaseDeploys, string WebUrl)
        {
            id = Id;
            version = Version;
            projectid = ProjectId;
            channelid = ChannelId;
            assembled = Assembled;
            releasenotes = ReleaseNotes;
            releaseDeploys = ReleaseDeploys;
            webUrl = WebUrl;
        }
    }
}