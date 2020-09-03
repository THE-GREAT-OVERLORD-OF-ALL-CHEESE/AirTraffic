using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_FlyOffMap : TrafficTask_Base
{
    public override bool IsTaskCompleted(Traffic_AI ai)
    {
        return false;
    }

    public override void StartTask(Traffic_AI ai)
    {
        Debug.Log("Starting fly off map task!");
        ai.waypoint.GetTransform().position = VTMapManager.GlobalToWorldPoint(AirTraffic.PointOnMapRadius());
        ai.aircraft.SetOrbitNow(ai.waypoint, 10000, AirTraffic.cruisingAltitudes.Random());
        base.StartTask(ai);
    }
}
