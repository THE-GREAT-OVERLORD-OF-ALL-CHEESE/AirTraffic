using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAircraft_E4 : TrafficAircraft_Base
{
    public override GameObject SpawnAircraft()
    {
        GameObject plane = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab("E-4"));
        plane.GetComponentInChildren<Radar>().gameObject.SetActive(false);
        return plane;
    }
}