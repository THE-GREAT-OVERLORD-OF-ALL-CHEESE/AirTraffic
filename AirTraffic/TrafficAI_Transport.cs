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

    public KinematicPlane kPlane;
    public FuelTank fuelTank;

    public float normalSpeed;

    //public Animator doorAnimator;
    //public float doorPosition;

    void Awake() {
        aircraft = GetComponent<AIAircraftSpawn>();
        pilot = GetComponent<AIPilot>();
        rb = GetComponent<Rigidbody>();
        //doorAnimator = GetComponentInChildren<Animator>();

        gearAnimator = GetComponentInChildren<GearAnimator>();
        tailHook = GetComponentInChildren<Tailhook>();
        catHook = GetComponentInChildren<CatapultHook>();
        refuelPort = GetComponentInChildren<RefuelPort>();
        wingRotator = pilot.wingRotator;

        kPlane = GetComponent<KinematicPlane>();
        fuelTank = GetComponent<FuelTank>();

        waypoint = new Waypoint();
        GameObject waypointObject = new GameObject();
        waypointObject.AddComponent<FloatingOriginTransform>();
        waypoint.SetTransform(waypointObject.transform);

        tilter = GetComponent<TiltController>();

        normalSpeed = pilot.navSpeed;

        GetComponent<Health>().OnDeath.AddListener(OnDeath);
    }

    void FixedUpdate() {
        UpdateTask();
        if (AirTraffic.DistanceFromOrigin(VTMapManager.WorldToGlobalPoint(transform.position)) > AirTraffic.trafficRadius * 1.01f)
        {
            Debug.Log(gameObject.name + " went outta bounds, respawning elsewhere");
            Vector3D pos = AirTraffic.PointOnCruisingRadius();
            Vector3 dir = -pos.toVector3;
            dir.y = 0;
            Spawn(pos, dir);
        }
    }

    public override void Spawn(Vector3D pos, Vector3 dir) {
        base.Spawn(pos, dir);
        rb.velocity = aircraft.transform.forward * pilot.navSpeed;

        //aircraft.SetOrbitNow(waypoint, 10000, AirTraffic.cruisingAltitudes.Random());
        pilot.CommandCancelOverride();

        gearAnimator.RetractImmediate();
        tailHook?.RetractHook();
        catHook?.SetState(0);
        wingRotator?.SetDefault();
        refuelPort.Close();

        kPlane.SetToKinematic();

        fuelTank.SetNormFuel(1);
    }

    public override void OnStartTask()
    {
        if (tilter != null)
        {
            tilter.SetTiltImmediate(90);
        }
    }

    void OnDeath() {
        Debug.Log(gameObject.name + " just died");
        if (currentTask != -1) {
            AirTraffic.potentialTasks[currentTask].EndTask(this);
        }
        AirTraffic.activeAircraftAmmount--;
    }
}
