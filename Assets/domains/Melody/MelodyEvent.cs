using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MelodyEvent
{
    public int index;
    public float timing;
    public List<int> ropeIndexes;
    public float length = 0;
    public List<MelodyInstanceAtRopeIndex> instancesAtRopeIndex = new List<MelodyInstanceAtRopeIndex>();
}

public class MelodyInstanceAtRopeIndex
{
    public int RopeIndex { get; set; }
    public GameObject Instance { get; set; }
}