using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAircraft_Bomber : TrafficAircraft_Base
{
    public override GameObject SpawnAircraft()
    {
        GameObject plane = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab("ABomberAI"));
        plane.name = "ABomberAI";
        return plane;
    }
}