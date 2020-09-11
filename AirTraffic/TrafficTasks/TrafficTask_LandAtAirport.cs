using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_LandAtAirport : TrafficTask_Transport
{
    public AirportReference airport;
    public float maxMass;
    public float maxSize;
    public bool vtolOnly;

    public TrafficTask_LandAtAirport(string newName, AirportReference airport, float maxMass, float maxSize, bool vtolOnly) : base(newName)
    {
        this.airport = airport;
        this.maxMass = maxMass;
        this.maxSize = maxSize;
        this.vtolOnly = vtolOnly;

        maxPerTask = AirTraffic.maxAircraftPerAirportTask;
    }

    public override bool CanStartTask(TrafficAI_Base ai)
    {
        if (base.CanStartTask(ai))
        {
            TrafficAI_Transport transportAI = (TrafficAI_Transport)ai;
            return transportAI.pilot.parkingSize < maxSize && (transportAI.rb.mass < maxMass || (vtolOnly && transportAI.pilot.isVtol)) && ParkingAvailable(transportAI.pilot.parkingSize);
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
        Debug.Log("Starting landing at airport task!");
        Debug.Log("Trying to land on " + airport.id);
        TrafficAI_Transport transportAI = (TrafficAI_Transport)ai;
        transportAI.aircraft.RearmAt(airport);
        base.StartTask(ai);
    }

    bool ParkingAvailable(float aircraftSize) {
        foreach (AirportManager.ParkingSpace parkingSpace in airport.GetAirport().parkingSpaces)
        {
            if (parkingSpace.parkingSize > aircraftSize && parkingSpace.occupiedBy == null)
            {
                return true;
            }
        }
        return false;
    }
}
