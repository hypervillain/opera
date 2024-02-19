using UnityEngine;
using Obi;

public static class RopeHelpers
{
    public static int GetElementIndexByRopeLengthPercentage(ObiRope rope, float percentage)
    {
        return Mathf.FloorToInt((rope.elements.Count - 1) * Mathf.Clamp01(percentage));
    }

    public static (Vector3, Vector3) GetParticlePositionByIndex(ObiRope rope, int index)
    {
        int particlePositionIndex = rope.elements[index < 0 ? rope.elements.Count + index : index].particle1;
        Vector3 position = rope.solver.positions[particlePositionIndex];
        Vector3 worldPosition = rope.transform.TransformPoint(position);
        // Debug.Log($"Element Index: {index}, Particle Position Index: {particlePositionIndex}, Local Position: {position}, World Position: {worldPosition}");
        return (position, worldPosition);
    }

    public static (Vector3, Vector3) GetParticlePositionByRopeLengthPercentage(ObiRope rope, float percentage)
    {
        return GetParticlePositionByIndex(rope, GetElementIndexByRopeLengthPercentage(rope, percentage));
    }

}