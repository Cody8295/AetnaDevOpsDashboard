using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class MachineList
    {
        public List<Machine> machines;
        public MachineList() { machines = new List<Machine>(); }
        
        public void add(Machine m) { machines.Add(m); }
    }
}