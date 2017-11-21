using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class ReleaseList
    {
        public List<Release> releaseList;
        public ReleaseList() { releaseList = new List<Release>(); }
        public void add(Release r) { releaseList.Add(r); }
    }
}