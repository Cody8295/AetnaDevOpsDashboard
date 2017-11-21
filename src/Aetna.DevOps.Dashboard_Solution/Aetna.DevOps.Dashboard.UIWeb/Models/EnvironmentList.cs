using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class EnvironmentList
    {
        public List<Environment> environments;
        public EnvironmentList() { environments = new List<Environment>(); }
        public void add(Environment e) { environments.Add(e); }
    }
}