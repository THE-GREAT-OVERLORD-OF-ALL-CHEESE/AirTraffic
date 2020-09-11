using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TrafficTask_CAP_Scout : TrafficTask_CAP
{
    public Vector3D point;
    public float idleTimer;
    public float radius = 3000;
    public float alt = 500;
    public bool arived;

    public TrafficTask_CAP_Scout(string newName, Vector3D point) : base(newName)
    {
        maxPerTask = 1;
        this.point = point;
    }

    public override bool IsTaskCompleted(TrafficAI_Base ai)
    {
        return idleTimer < 0;
    }

    public override void StartTask(TrafficAI_Base ai)
    {
        Debug.Log("Starting scout airbase!");
        TrafficAI_CAP capAI = (TrafficAI_CAP)ai;
        ai.waypoint.GetTransform().position = VTMapManager.GlobalToWorldPoint(point);
        capAI.Orbit(ai.waypoint, radius, alt);
        idleTimer = UnityEngine.Random.Range(30f, 120f);
        arived = false;
        base.StartTask(ai);
    }

    public override void UpdateTask(TrafficAI_Base ai)
    {
        TrafficAI_CAP capAI = (TrafficAI_CAP)ai;
        Vector3 flatPos = capAI.formation[0].transform.position;
        flatPos.y = 0;
        Vector3 flatWaypoint = ai.waypoint.GetTransform().position;
        flatPos.y = 0;

        if ((flatPos - flatWaypoint).magnitude < radius * 1.2f)
        {
            arived = true;
        }
        if (arived) {
            idleTimer -= Time.fixedDeltaTime;
        }
    }
}
