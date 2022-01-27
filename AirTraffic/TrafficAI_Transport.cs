using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAI_Transport : TrafficAI_Base
{
    public AIAircraftSpawn aircraft;
    public AIPilot pilot;
    public Rigidbody rb;
    public TiltController tilter;

    public GearAnimator gearAnimator;
    public Tailhook tailHook;
    public CatapultHook catHook;
    public RefuelPort refuelPort;
    public RotationToggle wingRotator;
    public WeaponManager wm;

    public KinematicPlane kPlane;
    public FuelTank fuelTank;

    public Health health;

    public float normalSpeed;

    //public Animator doorAnimator;
    //public float doorPosition;

    private void Awake() {
        aircraft = GetComponent<AIAircraftSpawn>();
        pilot = GetComponent<AIPilot>();
        rb = GetComponent<Rigidbody>();
        //doorAnimator = GetComponentInChildren<Animator>();

        gearAnimator = GetComponentInChildren<GearAnimator>();
        tailHook = GetComponentInChildren<Tailhook>();
        catHook = GetComponentInChildren<CatapultHook>();
        refuelPort = GetComponentInChildren<RefuelPort>();
        wingRotator = pilot.wingRotator;
        wm = GetComponentInChildren<WeaponManager>();

        kPlane = GetComponent<KinematicPlane>();
        fuelTank = GetComponent<FuelTank>();

        waypoint = new Waypoint();
        GameObject waypointObject = new GameObject();
        waypointObject.AddComponent<FloatingOriginTransform>();
        waypoint.SetTransform(waypointObject.transform);

        tilter = GetComponent<TiltController>();

        normalSpeed = pilot.navSpeed;

        health = GetComponent<Health>();
        health.OnDeath.AddListener(OnDeath);
    }

    protected override void UpdateTask() {
        base.UpdateTask();
        float maxDistance = AirTraffic.GetTrafficRadius() * 1.01f;

        if (AirTraffic.DistanceFromOrigin(VTMapManager.WorldToGlobalPoint(transform.position)) > maxDistance)
        {
            if (aircraft.aiPilot.commandState == AIPilot.CommandStates.Orbit)
            {
                Debug.Log(gameObject.name + $" went outta bounds {AirTraffic.DistanceFromOrigin(VTMapManager.WorldToGlobalPoint(transform.position))}m away from center, respawning elsewhere");
                Vector3D pos = AirTraffic.PointOnCruisingRadius();
                Vector3 dir = (AirTraffic.GetPlayerPosition() - pos).toVector3;
                dir.y = 0;
                Spawn(pos, dir);
            }
            else {
                if (health.normalizedHealth != 0) {
                    Debug.Log(gameObject.name + " went outta bounds untintentionally, destroying them for our saftey");
                    health.Kill();
                }
            }
        }
    }

    public override void Spawn(Vector3D pos, Vector3 dir) {
        base.Spawn(pos, dir);
        kPlane.SetToDynamic();
        rb.velocity = aircraft.transform.forward * pilot.navSpeed;
        kPlane.SetToKinematic();

        pilot.CommandCancelOverride();

        gearAnimator?.RetractImmediate();
        tailHook?.RetractHook();
        catHook?.SetState(0);
        wingRotator?.SetDefault();
        refuelPort?.Close();

        kPlane.SetToKinematic();

        fuelTank?.SetNormFuel(1);
    }

    public override void OnStartTask()
    {
        if (tilter != null)
        {
            tilter.SetTiltImmediate(90);
        }
    }

    public void OnDeath() {
        Debug.Log(gameObject.name + " just died");
        if (currentTask != -1) {
            AirTraffic.potentialTasks[currentTask].EndTask(this);
        }
        AircraftSpawner.activeAircraft.Remove(this);

        if (waypoint.GetTransform() != null)
        {
            Destroy(waypoint.GetTransform().gameObject);
        }
        else {
            Debug.Log("Couldn't destroy waypoint, it was null");
        }
        Destroy(this);
        if (aircraft.unitSpawner != null)
        {
            Destroy(aircraft.unitSpawner.gameObject);
        }
        else {
            Debug.Log("Couldn't destroy unit spawner, it was null");
        }
    }
}
