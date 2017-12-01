using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class MachineList
    {
        public List<Machine> Machines;
        public MachineList() { Machines = new List<Machine>(); }
        
        public void Add(Machine m) { Machines.Add(m); }

        public MachineList Clone()
        {
            MachineList newMachineList = new MachineList();
            foreach (Deploy machine in Machines)
            {
                newMachineList.Add(machine.Clone());
            }
            return newMachineList;
        }
    }
}