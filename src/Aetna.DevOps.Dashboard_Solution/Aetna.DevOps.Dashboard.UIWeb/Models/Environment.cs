using System.Collections.Generic;
using System.Diagnostics;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Environment : OctopusModel<Environment>
    {
        public string Id;
        public string Name;
        public string Description;
        public List<Machine> Machines;

        public Environment(string id, string name, string description, List<Machine> machines)
        {
            Id = id;
            Name = name;
            Description = description;
            Machines = machines;
        }

        public override string ToString()
        {
            return Name + ":" + Machines.Count;
        }

        public bool Equals(Environment other)
        {
            return (Id == other.Id && Name == other.Name && Description == other.Description && Machines.DeepEquals<Machine>(other.Machines));
        }
    }
}