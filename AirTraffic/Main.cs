using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;


public class AirTraffic : VTOLMOD
{
    public static GameObject player;
    public VTOLScenes currentScene;

    public static int activeAircraftAmmount;

    public List<TrafficAircraft_Base> spawnableAircraft;
    public static List<TrafficTask_Base> potentialTasks;
    public static int maxAircraftPerAirportTask = 2;
    public static int maxAircraftPerRefuelTask = 2;

    public static float trafficRadius = 50000;//50000
    public static MinMax cruisingAltitudes = new MinMax(3048, 9144);

    public UnityAction<int> targetAircraftAmmount_changed;
    public int targetAircraftAmmount = 15;//was 15

    public UnityAction<bool> useTransportAV42_changed;
    public bool useTransportAV42 = true;//true

    public UnityAction<bool> useTransportBig_changed;
    public bool useTransportBig = true;//true

    public UnityAction<bool> useTransportDrone_changed;
    public bool useTransportDrone = true;//true

    public UnityAction<bool> useFighters_changed;
    public bool useFighters = true;//true

    public UnityAction<bool> useBomber_changed;
    public bool useBomber = true;//true

    public override void ModLoaded()
    {
        HarmonyInstance harmony = HarmonyInstance.Create("cheese.airtraffic");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        base.ModLoaded();
        VTOLAPI.SceneLoaded += SceneLoaded;
        VTOLAPI.MissionReloaded += MissionReloaded;

        UpdateTransportAircraft();

        Settings settings = new Settings(this);
        settings.CreateCustomLabel("Air Traffic Settings");

        settings.CreateCustomLabel("");
        settings.CreateCustomLabel("Transport Aircraft");

        targetAircraftAmmount_changed += targetAircraftAmmount_Setting;
        settings.CreateCustomLabel("Ammount of transport aircraft:");
        settings.CreateIntSetting("(Default = 15)", targetAircraftAmmount_changed);

        useTransportAV42_changed += useTransportAV42_Setting;
        settings.CreateCustomLabel("Allow AV-42 to spawn as transport:");
        settings.CreateBoolSetting("(Default = true)", useTransportAV42_changed, useTransportAV42);

        useTransportBig_changed += useTransportBig_Setting;
        settings.CreateCustomLabel("Allow big aircraft to spawn as transport:");
        settings.CreateCustomLabel("(Modified KC-49 and E-4)");
        settings.CreateBoolSetting("(Default = true)", useTransportBig_changed, useTransportBig);

        useTransportDrone_changed += useTransportDrone_Setting;
        settings.CreateCustomLabel("Allow amazoon drone to spawn as transport:");
        settings.CreateCustomLabel("(Modified refuel plane)");
        settings.CreateBoolSetting("(Default = true)", useTransportDrone_changed, useTransportDrone);

        useFighters_changed += useFighters_Setting;
        settings.CreateCustomLabel("Allow fighter jets to spawn as transport:");
        settings.CreateCustomLabel("(F/A-26 and F45s)");
        settings.CreateBoolSetting("(Default = true)", useFighters_changed, useFighters);

        useBomber_changed += useBomber_Setting;
        settings.CreateCustomLabel("Allow bombers to spawn as supersonic transport:");
        settings.CreateBoolSetting("(Default = true)", useFighters_changed, useFighters);

        settings.CreateCustomLabel("");
        settings.CreateCustomLabel("Please feel free to @ me on the discord if");
        settings.CreateCustomLabel("you think of any more features I could add!");

        VTOLAPI.CreateSettingsMenu(settings);
    }

    public void UpdateTransportAircraft() {
        spawnableAircraft = new List<TrafficAircraft_Base>();
        if (useTransportAV42)
        {
            spawnableAircraft.Add(new TrafficAircraft_Base());//its the base class everything inherits from, but also it is actually the AV-42
        }
        if (useTransportBig)
        {
            spawnableAircraft.Add(new TrafficAircraft_E4());
            spawnableAircraft.Add(new TrafficAircraft_KC49());
        }
        if (useTransportDrone)
        {
            spawnableAircraft.Add(new TrafficAircraft_MQ31());
        }
        if (useFighters)
        {
            spawnableAircraft.Add(new TrafficAircraft_FA26());
            spawnableAircraft.Add(new TrafficAircraft_F45());
        }
        if (useFighters)
        {
            spawnableAircraft.Add(new TrafficAircraft_FA26());
            spawnableAircraft.Add(new TrafficAircraft_F45());
        }
        if (useBomber)
        {
            //bombers suck at taxiing, so they cannot land
            spawnableAircraft.Add(new TrafficAircraft_Bomber());
        }        
    }

    public void targetAircraftAmmount_Setting(int newval)
    {
        targetAircraftAmmount = newval;
    }

    public void useTransportAV42_Setting(bool newval)
    {
        useTransportAV42 = newval;
        UpdateTransportAircraft();
    }

    public void useTransportBig_Setting(bool newval)
    {
        useTransportBig = newval;
        UpdateTransportAircraft();
    }

    public void useTransportDrone_Setting(bool newval)
    {
        useTransportDrone = newval;
        UpdateTransportAircraft();
    }

    public void useFighters_Setting(bool newval)
    {
        useFighters = newval;
        UpdateTransportAircraft();
    }

    public void useBomber_Setting(bool newval)
    {
        useBomber = newval;
        UpdateTransportAircraft();
    }

    void SceneLoaded(VTOLScenes scene)
    {
        currentScene = scene;
        switch (scene)
        {
            case VTOLScenes.Akutan:
            case VTOLScenes.CustomMapBase:
                StartCoroutine("SetupScene");
                break;
            default:
                break;
        }
    }

    private void MissionReloaded()
    {
        StartCoroutine("SetupScene");
    }

    private IEnumerator SetupScene()
    {
        while (VTMapManager.fetch == null || !VTMapManager.fetch.scenarioReady || FlightSceneManager.instance.switchingScene)
        {
            yield return null;
        }

        SetupTasks();
        InitialSpawnTraffic(targetAircraftAmmount);
    }

    void FixedUpdate() {
        if ((currentScene == VTOLScenes.Akutan || currentScene == VTOLScenes.CustomMapBase) && activeAircraftAmmount < targetAircraftAmmount && spawnableAircraft.Count > 0) {
            Debug.Log("We lost an aircraft somewhere, adding a new one!");
            Vector3D pos = PointOnCruisingRadius();
            Vector3 dir = -pos.toVector3;
            dir.y = 0;
            SpawnRandomAircraft(pos, dir);
        }
    }

    void SetupTasks()
    {
        Debug.Log("Setting up map tasks!");
        potentialTasks = new List<TrafficTask_Base>();

        potentialTasks.Add(new TrafficTask_FlyOffMap("fly off map"));
        
        foreach (string airportId in VTScenario.current.GetAllAirportIDs())
        {
            float maxSize = 0;
            float maxMass = 0;
            
            foreach (AirportManager.ParkingSpace parkingSpace in new AirportReference(airportId).GetAirport().parkingSpaces) {
                if (parkingSpace.parkingSize > maxSize) {
                    maxSize = parkingSpace.parkingSize;
                }
            }

            foreach (Runway runway in new AirportReference(airportId).GetAirport().runways)
            {
                if (runway.maxMass > maxMass)
                {
                    maxMass = runway.maxMass;
                }
            }

            potentialTasks.Add(new TrafficTask_LandAtAirport("land at airport " + new AirportReference(airportId).GetAirport().name, new AirportReference(airportId), maxMass, maxSize, new AirportReference(airportId).GetAirport().vtolOnlyLanding));
            Debug.Log("Added task land at " + airportId);
            Debug.Log("Maximum landing mass is " + maxMass);
            Debug.Log("Maximum landing size is " + maxSize);

            //potentialTasks.Add(new TrafficTask_CAP_Scout("scout airbase: " + new AirportReference(airportId).GetAirport().name, VTMapManager.WorldToGlobalPoint(new AirportReference(airportId).GetAirport().transform.position)));
        }

        int oilrigCount = 0;
        foreach (VTSODestructible oilrig in FindObjectsOfType<VTSODestructible>()) {
            Debug.Log("Added task land at oilrig " + oilrigCount);
            potentialTasks.Add(new TrafficTask_LandTemp("land at oilrig " + oilrigCount, oilrig.transform, new Vector3(49, 65, 48)));
            oilrigCount++;
        }

        foreach (RefuelPlane rp in FindObjectsOfType<RefuelPlane>())
        {
            Debug.Log("Added task air to air refuel at " + rp.name);
            //potentialTasks.Add(new TrafficTask_CAP_Refuel("refuel at " + rp.name, rp));
            potentialTasks.Add(new TrafficTask_Transport_Refuel("refuel at " + rp.name, rp));
        }
    }

    void InitialSpawnTraffic(int ammount) {
        Debug.Log("Spawing initial airtraffic");
        player = VTOLAPI.GetPlayersVehicleGameObject();

        //GameObject aircraft = new TrafficAircraft_CAP().SpawnAircraft();
        //float bearing2 = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        //aircraft.GetComponent<TrafficAI_CAP>().Spawn(PointInCruisingRadius(), new Vector3(Mathf.Sin(bearing2), 0, Mathf.Cos(bearing2)));

        activeAircraftAmmount = 0;
        
        for (int i = 0; i < targetAircraftAmmount; i++) {
            float bearing = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            SpawnRandomAircraft(PointInCruisingRadius(), new Vector3(Mathf.Sin(bearing), 0, Mathf.Cos(bearing)));
        }
    }

    void SpawnRandomAircraft(Vector3D pos, Vector3 dir) {
        if (spawnableAircraft.Count > 0)
        {
            GameObject aircraft = spawnableAircraft[UnityEngine.Random.Range(0, spawnableAircraft.Count)].SpawnAircraft();

            FloatingOriginTransform floatingTransform = aircraft.AddComponent<FloatingOriginTransform>();
            if (floatingTransform != null)
            {
                floatingTransform.SetRigidbody(aircraft.GetComponent<Rigidbody>());
            }
            aircraft.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

            TrafficAI_Transport ai = aircraft.AddComponent<TrafficAI_Transport>();
            GameObject unitSpawner = new GameObject();
            ai.aircraft.unitSpawner = unitSpawner.AddComponent<UnitSpawner>();
            Traverse unitSpawnerTraverse = Traverse.Create(ai.aircraft.unitSpawner);
            unitSpawnerTraverse.Field("_spawned").SetValue(true);
            Traverse aircraftSpawnerTraverse = Traverse.Create(ai.aircraft);
            aircraftSpawnerTraverse.Field("taxiSpeed").SetValue(ai.aircraft.aiPilot.taxiSpeed);
            ai.Spawn(pos, dir);

            Debug.Log("Spawned " + aircraft.name + " as air traffic");
            activeAircraftAmmount++;
        }
        else {
            Debug.Log("No aircraft were available, cannot spawn a traffic aircraft.");
        }
    }

    public static Vector3D GetPlayerPosition()
    {
        return VTMapManager.WorldToGlobalPoint(player.transform.position);
    }

    public static Vector3D PointInCruisingRadius()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle;
        Vector3D playerPos = GetPlayerPosition();

        return new Vector3D(randomCircle.x * trafficRadius + playerPos.x, cruisingAltitudes.Random(), randomCircle.y * trafficRadius + playerPos.z);
    }

    public static Vector3D PointOnCruisingRadius()
    {
        float bearing = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3D playerPos = GetPlayerPosition();

        return new Vector3D(Mathf.Sin(bearing) * trafficRadius + playerPos.x, cruisingAltitudes.Random(), Mathf.Cos(bearing) * trafficRadius + playerPos.z);
    }

    public static Vector3D PointOnMapRadius()
    {
        float bearing = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float mapRadius = VTMapManager.fetch.map.mapSize * 1500;

        return new Vector3D(Mathf.Sin(bearing) * mapRadius * 1.4f + mapRadius, cruisingAltitudes.Random(), Mathf.Cos(bearing) * mapRadius * 1.4f + mapRadius);
    }

    public static float DistanceFromOrigin(Vector3D otherPos)
    {
        Vector3D playerPos = GetPlayerPosition();
        playerPos.y = 0;
        otherPos.y = 0;
        return (float)(playerPos - otherPos).magnitude;
    }
}