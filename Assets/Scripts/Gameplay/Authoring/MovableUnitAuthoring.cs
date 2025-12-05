using CrossCut.Pathfinding.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class MovableUnitAuthoring : MonoBehaviour
{
    
}

class MovableUnitAuthoringBaker : Baker<MovableUnitAuthoring>
{
    public override void Bake(MovableUnitAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        MovementComponent movementComponent = new MovementComponent
        {
            CurrentLocation = new float3(float.MaxValue, float.MaxValue, float.MaxValue),
            Speed = 5f,
            IsMoving = false
        };

        AddComponent(entity, movementComponent);
        AddBuffer<PathBuffer>(entity);
    }
}
