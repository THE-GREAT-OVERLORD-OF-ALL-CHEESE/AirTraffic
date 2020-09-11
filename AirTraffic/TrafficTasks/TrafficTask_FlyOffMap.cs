using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_FlyOffMap : TrafficTask_Transport
{
    public TrafficTask_FlyOffMap(string newName) : base(newName)
    {

    }

    public override bool IsTaskCompleted(TrafficAI_Base ai)
    {
        return false;
    }

    public override void StartTask(TrafficAI_Base ai)
    {
        Debug.Log("Starting fly off map task!");
        TrafficAI_Transport ai2 = (TrafficAI_Transport)ai;
        ai2.waypoint.GetTransform().position = VTMapManager.GlobalToWorldPoint(AirTraffic.PointOnMapRadius());
        ai2.aircraft.SetOrbitNow(ai2.waypoint, 10000, AirTraffic.cruisingAltitudes.Random());
        base.StartTask(ai2);
    }
}
