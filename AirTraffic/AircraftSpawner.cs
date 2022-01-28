using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VTNetworking;
using VTOLVR.Multiplayer;

public static class AircraftSpawner
{
    public static string[] spawnableAircraftList = new string[] { "ABomberAI", "AV-42CAI", "E-4", "F-45A AI", "FA-26B AI", "KC-49", "MQ-31",
    "AEW-50", "AIUCAV", "ASF-30", "ASF-33", "ASF-58", "EBomberAI", "GAV-25" };

    public static List<TrafficAI_Transport> activeAircraft;

    public static void SpawnRandomAircraft(Vector3D pos, Vector3 dir)
    {
        if (spawnableAircraftList.Length > 0)
        {
            SpawnAircraft(spawnableAircraftList[UnityEngine.Random.Range(0, spawnableAircraftList.Length)], pos, dir);
        }
    }

    public static void SpawnAircraft(string aircraftName, Vector3D pos, Vector3 dir)
    {
        if (VTOLMPUtils.IsMultiplayer() && VTOLMPLobbyManager.isLobbyHost == false)
        {
            return;
        }

        if (UnitCatalogue.GetUnitPrefab(aircraftName) != null)
        {
            Vector3 spawnPos = VTMapManager.GlobalToWorldPoint(pos);
            Quaternion spawnRot = Quaternion.LookRotation(dir);

            Debug.Log("setting up unit spawner");
            GameObject unitSpawnerObj = new GameObject();
            UnitSpawner unitSpawner = unitSpawnerObj.AddComponent<UnitSpawner>();
            unitSpawner.unitID = aircraftName;

            unitSpawner.transform.position = spawnPos;
            unitSpawner.transform.rotation = spawnRot;

            if (VTOLMPLobbyManager.isLobbyHost)
            {
                Debug.Log("Network spawning a " + aircraftName + " at pos: " + pos.ToString());
                VTOLMPUnitManager.instance.StartCoroutine(MPSpawnRoutine(unitSpawner, pos, dir));
            }
            else
            {
                Debug.Log("Spawning a " + aircraftName + " at pos: " + pos.ToString());
                GameObject aircraftObj = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab(aircraftName), spawnPos, Quaternion.LookRotation(dir), null);

                SetupAircraft(unitSpawner, aircraftObj, pos, dir);
            }
        }
        else
        {
            Debug.Log("No aircraft were available, cannot spawn a traffic aircraft.");
        }
    }

    public static IEnumerator MPSpawnRoutine(UnitSpawner spawner, Vector3D pos, Vector3 dir)
    {
        string resourcePath = UnitCatalogue.GetUnit(spawner.unitID).resourcePath;
        VTNetworkManager.NetInstantiateRequest req = VTNetworkManager.NetInstantiate(resourcePath, spawner.transform.position, spawner.transform.rotation, true);
        while (!req.isReady)
        {
            yield return null;
        }
        GameObject obj = req.obj;

        SetupAircraft(spawner, obj, pos, dir);
    }

    public static void SetupAircraft(UnitSpawner spawn, GameObject aircraftObj, Vector3D pos, Vector3 dir)
    {
        Vector3 spawnPos = VTMapManager.GlobalToWorldPoint(pos);
        Quaternion spawnRot = Quaternion.LookRotation(dir);

        Debug.Log("adding traffic ai");
        TrafficAI_Transport ai = aircraftObj.AddComponent<TrafficAI_Transport>();

        aircraftObj.SetActive(false);

        Debug.Log("setting up floating origin");
        FloatingOriginTransform floatingTransform = aircraftObj.GetComponent<FloatingOriginTransform>();

        if (floatingTransform == null) {
            floatingTransform = aircraftObj.AddComponent<FloatingOriginTransform>();
        }

        floatingTransform.SetRigidbody(aircraftObj.GetComponent<Rigidbody>());
        aircraftObj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

        Debug.Log("setting up unit spawner");
        ai.aircraft.unitSpawner = spawn;

        Traverse unitSpawnerTraverse = Traverse.Create(ai.aircraft.unitSpawner);
        unitSpawnerTraverse.Field("_spawned").SetValue(true);
        Traverse aircraftSpawnerTraverse = Traverse.Create(ai.aircraft);
        aircraftSpawnerTraverse.Field("taxiSpeed").SetValue(ai.aircraft.aiPilot.taxiSpeed);

        Debug.Log("setting up engagers");
        IEngageEnemies[] engagers = ai.aircraft.gameObject.GetComponentsInChildrenImplementing<IEngageEnemies>(true);
        aircraftSpawnerTraverse.Field("engagers").SetValue(engagers);

        ai.aircraft.autoRTB = false;
        ai.aircraft.SetEngageEnemies(false);

        ai.pilot.startLanded = false;
        ai.pilot.actor.discovered = false;

        if (ai.pilot.detectionRadar != null)
        {
            ai.pilot.vt_radarEnabled = false;
            ai.pilot.playerComms_radarEnabled = false;
        }

        //if (ai.pilot.moduleRWR == null && SettingsManager.settings.giveRWR)
        if (false)
        {
            GameObject rwrObject = new GameObject();
            rwrObject.transform.parent = ai.transform;
            ModuleRWR rwr = rwrObject.AddComponent<ModuleRWR>();
            ai.pilot.moduleRWR = rwr;
        }

        Debug.Log("enabling aircraft");
        aircraftObj.SetActive(true);

        Debug.Log("setting up loadout");
        Loadout loadout = new Loadout();
        loadout.hpLoadout = new string[0];
        loadout.normalizedFuel = 1;
        loadout.cmLoadout = new int[] { 30, 30 };
        if (ai.wm)
        {
            ai.wm.EquipWeapons(loadout);
        }
        aircraftSpawnerTraverse.Field("loadout").SetValue(loadout);

        if (ai.pilot.actor.team == Teams.Enemy || false)
        {
            Debug.Log("forcing team to allied");

            ai.pilot.actor.team = Teams.Allied;
            TargetManager.instance.UnregisterActor(ai.pilot.actor);
            TargetManager.instance.RegisterActor(ai.pilot.actor);
        }

        Debug.Log("intialising AI");
        ai.Spawn(pos, dir);

        Debug.Log("Spawned " + aircraftObj.name + " as air traffic");
        activeAircraft.Add(ai);
    }
}
