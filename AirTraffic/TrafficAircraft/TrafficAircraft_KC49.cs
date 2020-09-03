using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAircraft_KC49 : TrafficAircraft_Base
{
    public override GameObject SpawnAircraft()
    {
        GameObject plane = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab("KC-49"));
        plane.GetComponentInChildren<RefuelPlane>().enabled = false;
        plane.GetComponentInChildren<ModuleTurret>().gameObject.SetActive(false);
        plane.GetComponentInChildren<RefuelGuideLights>().gameObject.SetActive(false);
        return plane;
    }
}