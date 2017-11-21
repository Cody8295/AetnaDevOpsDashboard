using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Machine
    {
        public string id;
        public string name;
        public string url;
        public List<string> environs;
        public string status;
        public string statusSummary;
        public string isInProcess;
        public Machine(string Id, string Name, string Url, System.Collections.Generic.List<string> Environs, string Status, string StatusSummary, string IsInProcess)
        {
            id = Id;
            name = Name;
            url = Url;
            environs = Environs;
            status = Status;
            statusSummary = StatusSummary;
            isInProcess = IsInProcess;
        }
    }
}