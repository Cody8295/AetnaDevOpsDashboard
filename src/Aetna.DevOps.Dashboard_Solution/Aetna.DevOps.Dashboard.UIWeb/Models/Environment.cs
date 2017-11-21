namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class Environment
    {
        public string id;
        public string name;
        public string description;
        public MachineList machines;

        public Environment(string Id, string Name, string Description, MachineList Machines)
        {
            id = Id;
            name = Name;
            description = Description;
            machines = Machines;
        }

        public override string ToString()
        {
            return name + ":" + machines.machines.Count;
        }
    }
}