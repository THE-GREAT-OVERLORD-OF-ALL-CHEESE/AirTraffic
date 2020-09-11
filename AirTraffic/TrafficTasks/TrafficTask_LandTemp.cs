using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_LandTemp : TrafficTask_Transport
{
    public Vector3D point;
    public float idleTimer;

    public TrafficTask_LandTemp(string newName, Transform helipad, Vector3 localOffset) : base(newName)
    {
        point = VTMapManager.WorldToGlobalPoint(helipad.TransformPoint(localOffset));
        maxPerTask = 1;
    }

    public override bool CanStartTask(TrafficAI_Base ai)
    {
        if (base.CanStartTask(ai))
        {
            TrafficAI_Transport transportAI = (TrafficAI_Transport)ai;
            return transportAI.pilot.isVtol;
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
        Debug.Log("Starting landing at temporary task!");
        TrafficAI_Transport transportAI = (TrafficAI_Transport)ai;
        ai.waypoint.GetTransform().position = VTMapManager.GlobalToWorldPoint(point);
        transportAI.aircraft.LandAtWpt(ai.waypoint);
        idleTimer = UnityEngine.Random.Range(15f,30f);
        base.StartTask(ai);
    }

    public override void UpdateTask(TrafficAI_Base ai)
    {
        TrafficAI_Transport transportAI = (TrafficAI_Transport)ai;
        if (transportAI.pilot.commandState == AIPilot.CommandStates.Park) {
            idleTimer -= Time.fixedDeltaTime;
            if (idleTimer < 0) {
                transportAI.aircraft.TakeOff();
            }
        }
    }
}