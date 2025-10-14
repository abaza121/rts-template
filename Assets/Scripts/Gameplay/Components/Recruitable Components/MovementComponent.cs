using Unity.Entities;
using Unity.Mathematics;

public struct MovementComponent : IComponentData
{
    public quaternion CurrentDirection;
    public float3 CurrentLocation;
    public float3 Destination;
    public float Speed;
    public bool IsMoving;
}
