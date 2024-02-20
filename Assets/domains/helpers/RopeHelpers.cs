using UnityEngine;
using Obi;

public static class RopeHelpers
{
    public enum RopeDirection
    {
        Up,
        Down
    }
    public static int GetElementIndexByRopeLengthPercentage(ObiRope rope, float percentage, RopeDirection direction)
    {
        if (direction == RopeDirection.Up)
        {
            percentage = 1f - percentage;
        }
        return Mathf.FloorToInt((rope.elements.Count - 1) * Mathf.Clamp01(percentage));
    }

    public static (Vector3, Vector3) GetParticlePositionByIndex(ObiRope rope, int index)
    {
        int particlePositionIndex = rope.elements[index < 0 ? rope.elements.Count + index : index].particle1;
        Vector3 position = rope.solver.positions[particlePositionIndex];
        Vector3 worldPosition = rope.transform.TransformPoint(position);
        return (position, worldPosition);
    }

    public static (Vector3, Vector3) GetParticlePositionByRopeLengthPercentage(ObiRope rope, float percentage, RopeDirection direction)
    {
        return GetParticlePositionByIndex(rope, GetElementIndexByRopeLengthPercentage(rope, percentage, direction));
    }

}