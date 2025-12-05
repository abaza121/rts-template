using Unity.Entities;
using Unity.Mathematics;

public struct PathfindingGridObstacleComponent : IComponentData
{
    /// <summary>
    /// Represents the size of the bounds as a two-dimensional vector.
    /// </summary>
    public float2 BoundsSize;

    /// <summary>
    /// Specifies the offset from the center point, typically used to adjust positioning in two-dimensional space.
    /// </summary>
    public float2 CenterOffset;
}