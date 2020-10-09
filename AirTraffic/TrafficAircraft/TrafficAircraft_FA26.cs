using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAircraft_FA26 : TrafficAircraft_Base
{
    public override GameObject SpawnAircraft() {
        GameObject plane = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab("FA-26B AI"));
        return plane;
    }
}
