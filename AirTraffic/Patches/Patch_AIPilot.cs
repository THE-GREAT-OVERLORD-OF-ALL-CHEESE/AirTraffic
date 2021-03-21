using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Oculus.Platform;
using UnityEngine;


/*[HarmonyPatch(typeof(AIPilot), "ResetAtParking")]
class Patch_ActorExtensions_ResetAtParking
{
    [HarmonyPrefix]
    static bool Prefix(AIPilot __instance, out bool __result, AICarrierSpawn cSpawn, Transform parkingNodeTf)
    {
        if (__instance.gameObject.GetComponent<TrafficAI_Transport>() != null)
        {
            /*Debug.Log("Traffic aircraft is reseting at parking");

            foreach(CarrierSpawnPoint carrierSpawnPoint in cSpawn.spawnPoints) {
                if (carrierSpawnPoint.spawnTf == parkingNodeTf)
                {
                    __instance.aiSpawn.takeOffPath = carrierSpawnPoint.catapultPath;
                    __instance.aiSpawn.catapult = carrierSpawnPoint.catapult;
                    break;
                }
            }

            __result = true;
            return true;/
        }
        else
        {
            /*__result = true;
             return true;/
        }

        Debug.Log("Traffic aircraft is reseting at parking");
        if (__instance.aiSpawn && parkingNodeTf != null)
        {
            Debug.Log("We gave an AI spawn and a parking transform");
            AirTraffic.instance.StartCoroutine(AirTraffic.ResetAtParkingRoutine(__instance, cSpawn, parkingNodeTf));
            __result = true;
        }
        Debug.Log("We have an AI spawn and a parking transform");
        __result = false;
        return false;
    }
}*/