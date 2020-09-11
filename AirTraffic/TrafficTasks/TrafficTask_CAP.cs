using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_CAP : TrafficTask_Base
{
    public TrafficTask_CAP(string newName) : base(newName)
    {

    }

    public override bool CanStartTask(TrafficAI_Base ai)
    {
        TrafficAI_CAP capAI = ai as TrafficAI_CAP;
        if (capAI == null)
            return false;
        return base.CanStartTask(ai);
    }
}
