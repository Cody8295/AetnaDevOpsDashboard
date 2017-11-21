using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class DeployList
    {
        public List<Deploy> deploys;
        public DeployList() { deploys = new List<Deploy>(); }
        public void add(Deploy d) { deploys.Add(d); }
    }
}