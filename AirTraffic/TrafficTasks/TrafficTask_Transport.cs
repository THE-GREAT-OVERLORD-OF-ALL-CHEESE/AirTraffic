using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_Transport : TrafficTask_Base
{
    public TrafficTask_Transport(string newName) : base(newName)
    {

    }

    public override bool CanStartTask(TrafficAI_Base ai)
    {
        TrafficAI_Transport transportAI = ai as TrafficAI_Transport;
        if (transportAI == null)
            return false;
        return base.CanStartTask(ai);
    }
}
