using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Traffic_AI : MonoBehaviour
{
    public AIAircraftSpawn aircraft;
    public AIPilot pilot;
    public Rigidbody rb;
    public TiltController tilter;

    public int lastTaskID = -1;
    public int currentTask = -1;

    public Waypoint waypoint;

    void Awake() {
        aircraft = GetComponent<AIAircraftSpawn>();
        pilot = GetComponent<AIPilot>();
        rb = GetComponent<Rigidbody>();

        waypoint = new Waypoint();
        GameObject waypointObject = new GameObject();
        waypointObject.AddComponent<FloatingOriginTransform>();
        waypoint.SetTransform(waypointObject.transform);

        tilter = GetComponent<TiltController>();

        GetComponent<Health>().OnDeath.AddListener(OnDeath);
    }

    void FixedUpdate() {
        if (AirTraffic.DistanceFromOrigin(VTMapManager.WorldToGlobalPoint(transform.position)) > AirTraffic.trafficRadius * 1.01f) {
            Debug.Log(gameObject.name + " went outta bounds, respawning elsewhere");
            Vector3D pos = AirTraffic.PointOnCruisingRadius();
            Vector3 dir = -pos.toVector3;
            dir.y = 0;
            Spawn(pos, dir);
        }

        if (currentTask == -1)
        {
            int taskID = UnityEngine.Random.Range(0, AirTraffic.potentialTasks.Count);

            if (AirTraffic.potentialTasks[taskID].CanStartTask(this)) {
                currentTask = taskID;
                lastTaskID = taskID;
                AirTraffic.potentialTasks[currentTask].StartTask(this);
                if (tilter != null)
                {
                    tilter.SetTiltImmediate(90);
                }
            }
        }
        else {
            if (AirTraffic.potentialTasks[currentTask].IsTaskCompleted(this))
            {
                AirTraffic.potentialTasks[currentTask].EndTask(this);
                currentTask = -1;
            }
            else {
                AirTraffic.potentialTasks[currentTask].UpdateTask(this);
            }
        }
    }

    public void Spawn(Vector3D pos, Vector3 dir) {
        aircraft.transform.position = VTMapManager.GlobalToWorldPoint(pos);
        aircraft.transform.LookAt(aircraft.transform.position + dir);
        rb.velocity = aircraft.transform.forward * pilot.navSpeed;

        if (currentTask != -1) {
            AirTraffic.potentialTasks[currentTask].EndTask(this);
        }
        lastTaskID = -1;
        currentTask = -1;
        aircraft.SetOrbitNow(waypoint, 10000, AirTraffic.cruisingAltitudes.Random());

        GetComponentInChildren<GearAnimator>().RetractImmediate();
        GetComponent<KinematicPlane>().SetToKinematic();
    }

    void OnDeath() {
        Debug.Log(gameObject.name + " just died");
        AirTraffic.activeAircraftAmmount--;
    }
}
