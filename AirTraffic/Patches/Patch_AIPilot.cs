using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Oculus.Platform;
using UnityEngine;

[HarmonyPatch(typeof(AIAircraftSpawn), "TakeOff")]
class Patch_AIAircraftSpawn_TakeOff
{
    [HarmonyPrefix]
    static bool Prefix(AIAircraftSpawn __instance)
    {
        Traverse aiPilotTraverse = Traverse.Create(__instance.aiPilot);
        if ((bool)aiPilotTraverse.Field("takeOffAfterLanding").GetValue())
        {
            __instance.aiPilot.commandState = AIPilot.CommandStates.Park;
        }

        return true;
    }
}