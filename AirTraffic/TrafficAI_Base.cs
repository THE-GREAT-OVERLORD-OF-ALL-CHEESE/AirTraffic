using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficAI_Base : MonoBehaviour
{
    public int lastTaskID = -1;
    public int currentTask = -1;

    public float taskCompletionTimer;
    public float taskUpdateTimer;

    public Waypoint waypoint;

    void Awake() {
        waypoint = new Waypoint();
        GameObject waypointObject = new GameObject();
        waypointObject.AddComponent<FloatingOriginTransform>();
        waypoint.SetTransform(waypointObject.transform);

        taskUpdateTimer = UnityEngine.Random.value;
    }

    private void FixedUpdate()
    {
        UpdateAI();
    }

    protected virtual void UpdateAI() {
        taskUpdateTimer += Time.fixedDeltaTime;
        taskCompletionTimer += Time.fixedDeltaTime;
        if (taskUpdateTimer > 1)
        {
            UpdateTask();
        }
    }

    protected virtual void UpdateTask() {
        if (currentTask == -1)
        {
            int taskID = UnityEngine.Random.Range(0, AirTraffic.potentialTasks.Count);

            if (taskID != lastTaskID && AirTraffic.potentialTasks[taskID].CanStartTask(this)) {
                currentTask = taskID;
                lastTaskID = taskID;
                AirTraffic.potentialTasks[currentTask].StartTask(this);
                Debug.Log(gameObject.name + " starting task " + AirTraffic.potentialTasks[currentTask].taskName);
                taskCompletionTimer = 0;
                OnStartTask();
            }
        }
        else {
            if (AirTraffic.potentialTasks[currentTask].IsTaskCompleted(this) && taskCompletionTimer > 5)
            {
                AirTraffic.potentialTasks[currentTask].EndTask(this);
                Debug.Log(gameObject.name + " completed task " + AirTraffic.potentialTasks[currentTask].taskName + " in " + taskCompletionTimer + " secconds");
                currentTask = -1;
            }
            else {
                AirTraffic.potentialTasks[currentTask].UpdateTask(this, taskUpdateTimer);
            }
        }

        taskUpdateTimer = 0;
    }

    public virtual void Spawn(Vector3D pos, Vector3 dir) {
        transform.position = VTMapManager.GlobalToWorldPoint(pos);
        transform.LookAt(transform.position + dir);

        if (currentTask != -1) {
            AirTraffic.potentialTasks[currentTask].EndTask(this);
        }
        lastTaskID = -1;
        currentTask = -1;
    }

    public virtual void OnStartTask() {

    }
}
