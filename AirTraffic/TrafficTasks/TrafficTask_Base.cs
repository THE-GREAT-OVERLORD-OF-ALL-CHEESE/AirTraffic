using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficTask_Base
{
    public string taskName;
    public int maxPerTask = int.MaxValue;
    public List<TrafficAI_Base> aircraftDoingTask;

    public TrafficTask_Base(string newName)
    {
        aircraftDoingTask = new List<TrafficAI_Base>();
        taskName = newName;
    }

    public virtual bool CanStartTask(TrafficAI_Base ai) {
        return aircraftDoingTask.Count() < maxPerTask;
    }

    public virtual bool IsTaskCompleted(TrafficAI_Base ai)
    {
        return true;
    }

    public virtual void StartTask(TrafficAI_Base ai)
    {
        aircraftDoingTask.Add(ai);
        Debug.Log("Task started, there are now " + aircraftDoingTask.Count + " doing task: " + taskName);
    }

    public virtual void UpdateTask(TrafficAI_Base ai)
    {

    }

    public virtual void EndTask(TrafficAI_Base ai)
    {
        aircraftDoingTask.Remove(ai);
        Debug.Log("Task completed, there are now " + aircraftDoingTask.Count + " doing task: " + taskName);
    }
}
