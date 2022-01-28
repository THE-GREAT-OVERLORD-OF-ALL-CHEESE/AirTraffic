using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(AIPilot), "TakeOffCarrier")]
class Patch_AIPilot_TakeOffCarrier
{
    [HarmonyPrefix]
    static bool Prefix(AIPilot __instance)
    {
        __instance.commandState = AIPilot.CommandStates.Park;
        return true;
    }
}