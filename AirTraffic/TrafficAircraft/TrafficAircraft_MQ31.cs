using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAircraft_MQ31 : TrafficAircraft_Base
{
    public override GameObject SpawnAircraft()
    {
        GameObject plane = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab("MQ-31"));
        //plane.GetComponentInChildren<RefuelPlane>().enabled = false;
        return plane;
    }
}