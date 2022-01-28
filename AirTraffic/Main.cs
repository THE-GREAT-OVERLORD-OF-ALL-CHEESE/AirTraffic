using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Valve.Newtonsoft.Json;
using VTNetworking;
using VTOLVR.Multiplayer;

public class AirTraffic : VTOLMOD
{
    public static AirTraffic instance;

    public VTOLScenes currentScene;

    public static List<TrafficTask_Base> potentialTasks;


    public static int maxAircraftPerAirportTask = 2;
    public static int maxAircraftPerRefuelTask = 2;

    public static float trafficRadius = 50000;//50000
    public static MinMax cruisingAltitudes = new MinMax(3048, 9144);
    public static float mapRadius;

    public class AirTrafficSettings
    {
        public int targetAircraftAmmount = 15;//was 15

        public bool mpTestMode = false;//false
    }

    public UnityAction<int> targetAircraftAmmount_changed;
    public UnityAction<bool> useTransportAV42_changed;
    public UnityAction<bool> useTransportBig_changed;
    public UnityAction<bool> useTransportDrone_changed;
    public UnityAction<bool> useFighters_changed;
    public UnityAction<bool> useBomber_changed;
    public UnityAction<bool> useEnemy_changed;
    public UnityAction<bool> mpTestMode_changed;

    public static AirTrafficSettings settings;
    public bool settingsChanged;

    public bool akutan = false;

    public float updateTimer;

    public override void ModLoaded()
    {
        HarmonyInstance harmony = HarmonyInstance.Create("cheese.airtraffic");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        base.ModLoaded();
        VTOLAPI.SceneLoaded += SceneLoaded;
        VTOLAPI.MissionReloaded += MissionReloaded;

        settings = new AirTrafficSettings();
        LoadFromFile();

        Settings modSettings = new Settings(this);
        modSettings.CreateCustomLabel("Air Traffic Settings");

        modSettings.CreateCustomLabel("");
        modSettings.CreateCustomLabel("Transport Aircraft");

        targetAircraftAmmount_changed += targetAircraftAmmount_Setting;
        modSettings.CreateCustomLabel("Ammount of transport aircraft:");
        modSettings.CreateIntSetting("(Default = 15)", targetAircraftAmmount_changed, settings.targetAircraftAmmount);

        mpTestMode_changed += mpTestMode_Setting;
        modSettings.CreateCustomLabel("MP Test Mode:");
        modSettings.CreateBoolSetting("(Default = false)", mpTestMode_changed, settings.mpTestMode);
        modSettings.CreateCustomLabel("This spreads the aircraft across the entire map instead of just near the player.");
        modSettings.CreateCustomLabel("DO NOT USE, IT IS NOT NECESSARY FOR MP");

        modSettings.CreateCustomLabel("");
        modSettings.CreateCustomLabel("Please feel free to @ me on the discord if");
        modSettings.CreateCustomLabel("you think of any more features I could add!");

        VTOLAPI.CreateSettingsMenu(modSettings);

        instance = this;
    }

    public void targetAircraftAmmount_Setting(int newval)
    {
        settingsChanged = true;
        settings.targetAircraftAmmount = newval;
    }

    public void mpTestMode_Setting(bool newval)
    {
        settingsChanged = true;
        settings.mpTestMode = newval;
    }

    private void SceneLoaded(VTOLScenes scene)
    {
        CheckSave();
        currentScene = scene;
        switch (scene)
        {
            case VTOLScenes.Akutan:
                akutan = true;
                StartCoroutine("SetupScene");
                break;
            case VTOLScenes.CustomMapBase:
                akutan = true;
                StartCoroutine("SetupScene");
                break;
            default:
                break;
        }
    }

    private void MissionReloaded()
    {
        CheckSave();
        StartCoroutine("SetupScene");
    }

    private void OnApplicationQuit()
    {
        CheckSave();
    }

    private void CheckSave()
    {
        Debug.Log("Checking if settings were changed.");
        if (settingsChanged)
        {
            Debug.Log("Settings were changed, saving changes!");
            SaveToFile();
        }
    }

    public void LoadFromFile()
    {
        string address = ModFolder;
        Debug.Log("Checking for: " + address);

        if (Directory.Exists(address))
        {
            Debug.Log(address + " exists!");
            try
            {
                Debug.Log("Checking for: " + address + @"\settings.json");
                string temp = File.ReadAllText(address + @"\settings.json");

                settings = JsonConvert.DeserializeObject<AirTrafficSettings>(temp);
                settingsChanged = false;
            }
            catch
            {
                Debug.Log("no json found, making one");
                SaveToFile();
            }
        }
        else
        {
            Debug.Log("Mod folder not found?");
        }
    }

    public void SaveToFile()
    {
        string address = ModFolder;
        Debug.Log("Checking for: " + address);

        if (Directory.Exists(address))
        {
            Debug.Log("Saving settings!");
            File.WriteAllText(address + @"\settings.json", JsonConvert.SerializeObject(settings));
            settingsChanged = false;
        }
        else
        {
            Debug.Log("Mod folder not found?");
        }
    }

    private IEnumerator SetupScene()
    {
        while (VTMapManager.fetch == null || !VTMapManager.fetch.scenarioReady || FlightSceneManager.instance.switchingScene)
        {
            yield return null;
        }

        mapRadius = VTMapManager.fetch.map.mapSize * 1500;
        if (VTOLMPUtils.IsMultiplayer() || settings.mpTestMode) {
            Debug.Log("mp mode: airtraffic will operate map wide");
        }
        SetupTasks();
        InitialSpawnTraffic(settings.targetAircraftAmmount);
    }

    private void FixedUpdate() {
        if (VTOLMPUtils.IsMultiplayer() && VTOLMPLobbyManager.isLobbyHost == false)
            return;

        updateTimer += Time.fixedDeltaTime;
        if (updateTimer > 5) {
            updateTimer = 0;
            if ((currentScene == VTOLScenes.Akutan || currentScene == VTOLScenes.CustomMapBase || currentScene == VTOLScenes.CustomMapBase_OverCloud) && AircraftSpawner.activeAircraft.Count < settings.targetAircraftAmmount && AircraftSpawner.spawnableAircraftList.Length > 0)
            {
                Debug.Log("We lost an aircraft somewhere, adding a new one!");
                Vector3D pos = PointOnCruisingRadius();
                Vector3 dir = -pos.toVector3;
                dir.y = 0;
                SpawnRandomAircraft(pos, dir);
            }
        }
    }

    void SetupTasks()
    {
        Debug.Log("Setting up map tasks!");
        potentialTasks = new List<TrafficTask_Base>();

        potentialTasks.Add(new TrafficTask_FlyOffMap("fly off map"));
        
        foreach (string airportId in VTScenario.current.GetAllAirportIDs())
        {
            AirportManager airport = new AirportReference(airportId).GetAirport();

            float maxSize = 0;
            float maxMass = 0;
            
            foreach (AirportManager.ParkingSpace parkingSpace in airport.parkingSpaces) {
                if (parkingSpace.parkingSize > maxSize) {
                    maxSize = parkingSpace.parkingSize;
                }
            }

            foreach (Runway runway in airport.runways)
            {
                if (runway.maxMass > maxMass)
                {
                    maxMass = runway.maxMass;
                }
            }

            potentialTasks.Add(new TrafficTask_LandAtAirport("land at airport " + airport.name, new AirportReference(airportId), maxMass, maxSize, airport.vtolOnlyLanding));
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
        if (VTOLMPUtils.IsMultiplayer() && VTOLMPLobbyManager.isLobbyHost == false) {
            return;
        }

        AircraftSpawner.activeAircraft = new List<TrafficAI_Transport>();
        
        for (int i = 0; i < settings.targetAircraftAmmount; i++) {
            float bearing = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            SpawnRandomAircraft(PointInCruisingRadius(), new Vector3(Mathf.Sin(bearing), 0, Mathf.Cos(bearing)));
        }
    }

    private void SpawnRandomAircraft(Vector3D pos, Vector3 dir) {
        AircraftSpawner.SpawnRandomAircraft(pos, dir);
    }

    public static Vector3D GetPlayerPosition()
    {
        if ((VTOLMPUtils.IsMultiplayer() || settings.mpTestMode) && instance.akutan == false)
        {
            return new Vector3D(mapRadius, 0, mapRadius);
        }
        else
        {
            if (FlightSceneManager.instance != null && FlightSceneManager.instance.playerActor != null)
            {
                return VTMapManager.WorldToGlobalPoint(FlightSceneManager.instance.playerActor.gameObject.transform.position);
            }
            else
            {
                return Vector3D.zero;
            }
        }
    }

    public static float GetTrafficRadius()
    {
        if ((VTOLMPUtils.IsMultiplayer() || settings.mpTestMode) && instance.akutan == false)
        {
            return mapRadius * 1.4f;
        }
        else
        {
            return trafficRadius;
        }
    }

    public static Vector3D PointInCruisingRadius()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle;
        Vector3D playerPos = GetPlayerPosition();

        return new Vector3D(randomCircle.x * GetTrafficRadius() + playerPos.x, cruisingAltitudes.Random(), randomCircle.y * GetTrafficRadius() + playerPos.z);
    }

    public static Vector3D PointOnCruisingRadius()
    {
        float bearing = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3D playerPos = GetPlayerPosition();

        return new Vector3D(Mathf.Sin(bearing) * GetTrafficRadius() + playerPos.x, cruisingAltitudes.Random(), Mathf.Cos(bearing) * GetTrafficRadius() + playerPos.z);
    }

    public static Vector3D PointOnMapRadius()
    {
        float bearing = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float mapRadius = VTMapManager.fetch.map.mapSize * 1500;

        return new Vector3D(Mathf.Sin(bearing * Mathf.Deg2Rad) * mapRadius * 1.5f + mapRadius, cruisingAltitudes.Random(), Mathf.Cos(bearing * Mathf.Deg2Rad) * mapRadius * 1.5f + mapRadius);
    }

    public static float DistanceFromOrigin(Vector3D otherPos)
    {
        Vector3D playerPos = GetPlayerPosition();
        playerPos.y = 0;
        otherPos.y = 0;
        return (float)(playerPos - otherPos).magnitude;
    }
}