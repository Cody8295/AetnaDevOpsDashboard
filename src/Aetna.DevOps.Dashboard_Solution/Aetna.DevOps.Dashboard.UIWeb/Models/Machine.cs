using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Machine
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

        public Machine Clone()
        {
            Machine newMachine = new Machine(Id, Name, Url, new List<string>(), Status, StatusSummary, IsInProcess);

            foreach (string environment in Environments)
            {
                newMachine.Environments.Add(environment);
            }

            return newMachine;
        }
    }
}