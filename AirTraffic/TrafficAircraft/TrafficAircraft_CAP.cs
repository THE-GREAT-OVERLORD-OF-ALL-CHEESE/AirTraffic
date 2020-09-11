using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAircraft_CAP : TrafficAircraft_Base
{
    public override GameObject SpawnAircraft()
    {
        GameObject cap = new GameObject();
        cap.AddComponent<TrafficAI_CAP>();
        return cap;
    }
}