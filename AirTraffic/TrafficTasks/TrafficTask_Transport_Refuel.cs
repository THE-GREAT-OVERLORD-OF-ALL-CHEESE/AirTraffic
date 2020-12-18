using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_Transport_Refuel : TrafficTask_Transport
{
    public RefuelPlane refuelPlane;

    public TrafficTask_Transport_Refuel(string newName, RefuelPlane refuelPlane) : base(newName)
    {
        this.refuelPlane = refuelPlane;

        maxPerTask = AirTraffic.maxAircraftPerRefuelTask;
    }

    public override bool CanStartTask(TrafficAI_Base ai)
    {
        if (base.CanStartTask(ai))
        {
            return refuelPlane != null;
        }
        else {
            return false;
        }
    }

    public override bool IsTaskCompleted(TrafficAI_Base ai)
    {
        TrafficAI_Transport transportAI = (TrafficAI_Transport)ai;
        return transportAI.pilot.commandState == AIPilot.CommandStates.Orbit;
    }

    public override void StartTask(TrafficAI_Base ai)
    {
        Debug.Log("Starting air to air refueling!");
        Debug.Log("Trying to refuel with " + refuelPlane.gameObject.name);
        TrafficAI_Transport transportAI = (TrafficAI_Transport)ai;
        transportAI.pilot.fuelTank.SetNormFuel(Mathf.Min(transportAI.pilot.fuelTank.fuel / transportAI.pilot.fuelTank.maxFuel, 0.5f));
        transportAI.pilot.GoRefuel(refuelPlane);
        base.StartTask(ai);
    }
}
