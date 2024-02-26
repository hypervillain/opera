using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "Game/Scene Data", order = 1)]
public class SceneData : ScriptableObject
{
    public int timeFactor;
    public int bpm;
    public int signature;
    public RopeHelpers.RopeDirection ropeDirection;
    public float tempoMarkerPositionPercentage;
    public int[] indexesToMarkTimeAt;
    public string FMODEventName;
    public string songDataName;
}