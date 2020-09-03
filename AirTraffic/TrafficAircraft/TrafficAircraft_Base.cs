using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAircraft_Base
{
    public virtual GameObject SpawnAircraft() {
        GameObject plane = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab("AV-42CAI"));//E-4 //KC-49 //ABomberAI //MQ-31
        return plane;
    }
}
