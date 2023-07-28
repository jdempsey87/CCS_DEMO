using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LevelData
{


    public string resource;
    public float[] position; // Use float[] instead of Vector3
    public float[] rotation; // Use float[] instead of Quaternion
    public float[] scale;    // Use float[] instead of Vector3


}
