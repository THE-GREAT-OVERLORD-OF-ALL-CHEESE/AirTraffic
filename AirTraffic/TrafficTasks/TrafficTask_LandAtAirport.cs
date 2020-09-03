using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_LandAtAirport : TrafficTask_Base
{
    public AirportReference airport;
    public float maxMass;
    public float maxSize;
    public bool vtolOnly;

    public TrafficTask_LandAtAirport(AirportReference airport, float maxMass, float maxSize, bool vtolOnly)
    {
        this.airport = airport;
        this.maxMass = maxMass;
        this.maxSize = maxSize;
        this.vtolOnly = vtolOnly;

        maxPerTask = AirTraffic.maxAircraftPerAirportTask;
    }

    public override bool CanStartTask(Traffic_AI ai)
    {
        return base.CanStartTask(ai) && ai.pilot.parkingSize < maxSize && (ai.rb.mass < maxMass || (vtolOnly && ai.pilot.isVtol)) && ParkingAvailable(ai.pilot.parkingSize);
    }

    public override bool IsTaskCompleted(Traffic_AI ai)
    {
        return ai.pilot.commandState == AIPilot.CommandStates.Orbit;
    }

    public override void StartTask(Traffic_AI ai)
    {
        Debug.Log("Starting landing at airport task!");
        Debug.Log("Trying to land on " + airport.id);
        ai.aircraft.RearmAt(airport);
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
