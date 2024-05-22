using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MemoryOptimizerPreset
{
    public bool changeDetectionEnabled;
    public int syncSteps;
    public float stepDelay;
    public List<string> selectedParameters = new List<string>();
}
