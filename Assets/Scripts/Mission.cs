using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission : MonoBehaviour
{
    public string TaskDescription { get; private set; }
    public float Duration { get; private set; }
    public bool IsMissionComplete { get; private set; }

    public Mission(string taskDescription, float duration)
    {
        TaskDescription = taskDescription;
        Duration = duration;
        IsMissionComplete = false;
    }

    public void CompleteMission()
    {
        IsMissionComplete = true;
    }
}
