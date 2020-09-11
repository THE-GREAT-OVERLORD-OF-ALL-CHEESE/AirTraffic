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

    public Animator doorAnimator;
    public float doorPosition;

    void Awake() {
        aircraft = GetComponent<AIAircraftSpawn>();
        pilot = GetComponent<AIPilot>();
        rb = GetComponent<Rigidbody>();
        doorAnimator = GetComponentInChildren<Animator>();

        waypoint = new Waypoint();
        GameObject waypointObject = new GameObject();
        waypointObject.AddComponent<FloatingOriginTransform>();
        waypoint.SetTransform(waypointObject.transform);

        tilter = GetComponent<TiltController>();

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

        if (doorAnimator != null) {
            if (pilot.commandState == AIPilot.CommandStates.Park)
            {
                if (doorPosition < 1)
                {
                    doorAnimator.SetFloat("doorSpeed", 1f);
                    doorPosition += Time.fixedDeltaTime;
                }
                else
                {
                    doorAnimator.SetFloat("doorSpeed", 0f);
                }
            }
            else
            {
                if (doorPosition > 0)
                {
                    doorAnimator.SetFloat("doorSpeed", -1f);
                    doorPosition -= Time.fixedDeltaTime;
                }
                else
                {
                    doorAnimator.SetFloat("doorSpeed", 0f);
                }
            }
        }
    }

    public override void Spawn(Vector3D pos, Vector3 dir) {
        base.Spawn(pos, dir);
        rb.velocity = aircraft.transform.forward * pilot.navSpeed;

        //aircraft.SetOrbitNow(waypoint, 10000, AirTraffic.cruisingAltitudes.Random());
        pilot.CommandCancelOverride();

        GetComponentInChildren<GearAnimator>().RetractImmediate();
        GetComponent<KinematicPlane>().SetToKinematic();
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
        AirTraffic.activeAircraftAmmount--;
    }
}
