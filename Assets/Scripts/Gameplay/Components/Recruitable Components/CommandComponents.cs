using Unity.Entities;
using Unity.Mathematics;

public struct MovementCommandComponent : IComponentData
{
    public float3 TargetPosition;
    public Entity EntityToMove;
}

public struct AttackCommandComponent : IComponentData
{
    public Entity TargetEntity;
    public Entity AttackerEntity;
}

