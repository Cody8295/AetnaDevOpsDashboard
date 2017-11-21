namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Release
    {
        public string id, version, projectid, channelid, assembled, releasenotes;
        public Release(string Id, string Version, string ProjectId, string ChannelId, string Assembled, string ReleaseNotes)
        {
            id = Id;
            version = Version;
            projectid = ProjectId;
            channelid = ChannelId;
            assembled = Assembled;
            releasenotes = ReleaseNotes;
        }
    }
}