﻿using System.Collections.Generic;

namespace Aetna.DevOps.Dashboard.UIWeb.Models
{
    public class DeployList
    {
        public List<Deploy> Deploys;
        public DeployList() { Deploys = new List<Deploy>(); }
        public void Add(Deploy newDeploy) { Deploys.Add(newDeploy); }
    }
}