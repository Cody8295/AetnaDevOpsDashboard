﻿using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class EnvironmentList
    {
        public List<Environment> Environments;
        public EnvironmentList() { Environments = new List<Environment>(); }
        public void Add(Environment newEnvironment) { Environments.Add(newEnvironment); }
    }
}