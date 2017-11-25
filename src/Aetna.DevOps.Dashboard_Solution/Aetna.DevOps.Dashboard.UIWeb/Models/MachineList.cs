using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class MachineList
    {
        public List<Machine> Machines;
        public MachineList() { Machines = new List<Machine>(); }
        
        public void Add(Machine m) { Machines.Add(m); }
    }
}