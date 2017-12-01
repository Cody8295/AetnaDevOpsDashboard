namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Environment
    {
        public string Id;
        public string Name;
        public string Description;
        public MachineList Machines;

        public Environment(string id, string name, string description, MachineList machines)
        {
            Id = id;
            Name = name;
            Description = description;
            Machines = machines;
        }

        public override string ToString()
        {
            return Name + ":" + Machines.Machines.Count;
        }

        public Environment Clone()
        {
            return new Environment(Id, Name, Description, Machines.Clone());
        }
    }
}