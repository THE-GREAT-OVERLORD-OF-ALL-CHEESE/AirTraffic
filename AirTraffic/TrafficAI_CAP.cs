using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAI_CAP : TrafficAI_Base
{
    public class CAP_Aircraft {
        public GameObject gameObject;
        public Transform transform;
        public AIAircraftSpawn aircraft;
        public AIPilot pilot;
        public Rigidbody rb;

        public CAP_Aircraft()
        {
            Spawn();
        }

        void Spawn() {
            Debug.Log("Spawning CAP F/A-26B");
            gameObject = GameObject.Instantiate(UnitCatalogue.GetUnitPrefab("FA-26B AI"));
            gameObject.AddComponent<FloatingOriginTransform>();
            transform = gameObject.transform;
            aircraft = gameObject.GetComponent<AIAircraftSpawn>();
            pilot = gameObject.GetComponent<AIPilot>();
            rb = gameObject.GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            gameObject.GetComponent<KinematicPlane>().SetToKinematic();

            Loadout loadout = new Loadout();
            loadout.hpLoadout = new string[] {"af_gun", "af_aim9", "af_aim9", "af_amraamRail", "af_amraam", "af_amraam", "af_amraam", "af_amraam", "af_amraamRail", "af_aim9", "af_aim9", "fa26_droptank", "fa26_droptank", "fa26_droptank", "af_tgp", ""};
            loadout.cmLoadout = new int[] {120, 120};
            loadout.normalizedFuel = 1;
            gameObject.GetComponent<WeaponManager>().EquipWeapons(loadout);
        }
    }

    public List<CAP_Aircraft> formation;

    void Awake() {
        formation = new List<CAP_Aircraft>();
        for (int i = 0; i < 3; i++) {
            formation.Add(new CAP_Aircraft());
        }

        waypoint = new Waypoint();
        GameObject waypointObject = new GameObject();
        waypointObject.AddComponent<FloatingOriginTransform>();
        waypoint.SetTransform(waypointObject.transform);
    }

    void FixedUpdate() {
        UpdateTask();
    }

    public override void Spawn(Vector3D pos, Vector3 dir) {
        Debug.Log("Reseting aircraft");
        foreach (CAP_Aircraft plane in formation)
        {
            plane.aircraft.transform.position = VTMapManager.GlobalToWorldPoint(pos);
            plane.aircraft.transform.LookAt(plane.aircraft.transform.position + dir);
            plane.rb.velocity = plane.gameObject.transform.forward * plane.pilot.navSpeed;

            plane.gameObject.GetComponentInChildren<GearAnimator>().RetractImmediate();
            plane.gameObject.GetComponent<KinematicPlane>().SetToKinematic();
            Debug.Log("aircraft reset");
        }
    }

    public void Orbit(Waypoint waypoint, float radius, float alt)
    {
        for (int i = 0; i < formation.Count; i++)
        {
            if (i == 0)
            {
                formation[i].aircraft.SetOrbitNow(waypoint, radius, alt);
            }
            else
            {
                formation[i].pilot.CommandCancelOverride();
                formation[i].pilot.FormOnPilot(formation[0].pilot);
            }
        }
    }
}
