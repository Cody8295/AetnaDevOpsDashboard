using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ReleaseList
    {
        public List<Release> Releases;
        public ReleaseList() { Releases = new List<Release>(); }
        public void Add(Release r) { Releases.Add(r); }
    }
}