using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Oculus.Platform;
using UnityEngine;

[HarmonyPatch(typeof(ActorExtensions), "DebugName")]
class Patch_ActorExtensions_DebugName
{
    [HarmonyPrefix]
    static bool Prefix(ref string __result)
    {
        __result = "string lmao";
        return false;
    }
}

/*
[HarmonyPatch(typeof(AIPilot), "TaxiAirbaseNav")]
class Patch_AIPilot_TaxiAirbaseNav
{
    [HarmonyPrefix]
    static bool Prefix(List<AirbaseNavNode> navTfs, Runway tgtRunway, AIPilot __instance)
    {
        Debug.Log("This is the taxi patch");
        __instance.StartCoroutine(AirTraffic.AirbaseNavTaxiRoutine(navTfs, tgtRunway, __instance));
        return false;
    }
}
*/