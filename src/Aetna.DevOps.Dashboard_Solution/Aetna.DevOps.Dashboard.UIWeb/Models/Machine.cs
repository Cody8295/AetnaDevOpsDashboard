using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Machine : OctopusModel<Machine>
    {
        public string Id;
        public string Name;
        public string Url;
        public List<string> Environments;
        public string Status;
        public string StatusSummary;
        public string IsInProcess;
        public Machine(string id, string name, string url, List<string> environments, string status, string statusSummary, string isInProcess)
        {
            Id = id;
            Name = name;
            Url = url;
            Environments = environments;
            Status = status;
            StatusSummary = statusSummary;
            IsInProcess = isInProcess;
        }

        public bool Equals(Machine other)
        {
            return (Id == other.Id && Name == other.Name && Environments.DeepEquals(other.Environments) && Status == other.Status && StatusSummary == other.StatusSummary && IsInProcess == other.IsInProcess);
        }
    }
}