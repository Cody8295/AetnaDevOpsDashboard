using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Environment : Clonable<Environment>
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

        public Environment Clone()
        {
            return new Environment(Id, Name, Description, Machines.Clone());
        }
    }
}