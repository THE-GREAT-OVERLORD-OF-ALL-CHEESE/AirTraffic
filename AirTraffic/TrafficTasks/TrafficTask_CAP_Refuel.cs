using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_CAP_Refuel : TrafficTask_CAP
{
    public RefuelPlane refuelPlane;

    public TrafficTask_CAP_Refuel(string newName, RefuelPlane refuelPlane) : base(newName)
    {
        maxPerTask = 1;
        this.refuelPlane = refuelPlane;
    }

    public override bool IsTaskCompleted(TrafficAI_Base ai)
    {
        TrafficAI_CAP capAI = (TrafficAI_CAP)ai;
        foreach (TrafficAI_CAP.CAP_Aircraft plane in capAI.formation)
        {
            if (plane.pilot.commandState == AIPilot.CommandStates.AirRefuel)
                return false;
        }
        return true;
    }

    public override void StartTask(TrafficAI_Base ai)
    {
        Debug.Log("Starting air to air refueling!");
        TrafficAI_CAP capAI = (TrafficAI_CAP)ai;
        foreach (TrafficAI_CAP.CAP_Aircraft plane in capAI.formation)
        {
            plane.pilot.fuelTank.SetNormFuel(Mathf.Min(plane.pilot.fuelTank.fuel/ plane.pilot.fuelTank.maxFuel, 0.5f));
            plane.pilot.CommandCancelOverride();
            plane.pilot.GoRefuel(refuelPlane);

        }
        base.StartTask(ai);
    }
}
