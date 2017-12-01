using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ReleaseList
    {
        public List<Release> Releases;
        public ReleaseList() { Releases = new List<Release>(); }
        public void Add(Release r) { Releases.Add(r); }
        public ReleaseList Clone()
        {
            ReleaseList newReleaseList = new ReleaseList();
            foreach (Deploy release in Releases)
            {
                newReleaseList.Add(release.Clone());
            }
            return newReleaseList;
        }
    }
}