using CrossCut.Gameplay.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;

class BuildingAuthoring : MonoBehaviour
{
    public PhysicsShapeAuthoring PhysicsShapeAuthoring;
    public Transform SpawnLocation;
    public BuildingType Type = BuildingType.None;
}

class BuildingAuthoringBaker : Baker<BuildingAuthoring>
{
    public override void Bake(BuildingAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        var collider = authoring.PhysicsShapeAuthoring;
        var halfExtents = collider != null ? new float3(collider.GetBoxProperties().Size * 0.5f) : new float3(0.5f, 0.5f, 0.5f);
        var centerOffset = collider != null ? new float3(collider.GetBoxProperties().Center) : new float3(0.5f, 0.5f, 0.5f);

        AddComponent(entity, new BuildingBounds { HalfExtents = halfExtents , CenterOffset = centerOffset });

        if (authoring.SpawnLocation != null)
        {
            AddComponent(entity, new SpawnPositionComponent { Value = authoring.SpawnLocation.position });
        }

        switch (authoring.Type)
        {
            case BuildingType.Barracks:
                AddComponent<BarracksTag>(entity);
                break;
        }


        // Add buffer for child entities
        var buffer = AddBuffer<BuildingChild>(entity);
        foreach (var child in authoring.GetComponentsInChildren<MeshRenderer>())
        {
            if(child.gameObject.GetComponent<BuildingColorAuthoring>() == null)
            {
                child.gameObject.AddComponent<BuildingColorAuthoring>();
            }
                
            var childEntity = GetEntity(child.gameObject, TransformUsageFlags.Dynamic);
            buffer.Add(new BuildingChild { Value = childEntity });
        }
    }
}

public struct BuildingBounds : IComponentData
{
    public float3 HalfExtents;
    public float3 CenterOffset;
}

public struct BuildingChild : IBufferElementData
{
    public Entity Value;
}

public struct BarracksTag : IComponentData { }
