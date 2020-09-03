using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficTask_Base
{
    public int maxPerTask = int.MaxValue;
    public List<Traffic_AI> aircraftDoingTask;

    public TrafficTask_Base()
    {
        aircraftDoingTask = new List<Traffic_AI>();
    }

    public virtual bool CanStartTask(Traffic_AI ai) {
        return aircraftDoingTask.Count() < maxPerTask;
    }

    public virtual bool IsTaskCompleted(Traffic_AI ai)
    {
        return true;
    }

    public virtual void StartTask(Traffic_AI ai)
    {
        aircraftDoingTask.Add(ai);
        Debug.Log("There are now " + aircraftDoingTask.Count + " doing this task");
    }

    public virtual void UpdateTask(Traffic_AI ai)
    {

    }

    public virtual void EndTask(Traffic_AI ai)
    {
        aircraftDoingTask.Remove(ai);
        Debug.Log("There are now " + aircraftDoingTask.Count + " doing this task");
    }
}
