﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAircraft_GAV25 : TrafficAircraft_Base
{
    public override GameObject SpawnAircraft() {
        GameObject plane = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab("GAV-25"));

        Actor actor = plane.GetComponent<Actor>();
        actor.team = Teams.Allied;
        TargetManager.instance.UnregisterActor(actor);
        TargetManager.instance.RegisterActor(actor);

        return plane;
    }
}
