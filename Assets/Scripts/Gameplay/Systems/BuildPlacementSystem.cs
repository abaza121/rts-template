using CrossCut.Gameplay.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(InitializationSystemGroup))]
partial struct BuildPlacementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BuildingTokenComponent>();
    }

    // This system handles the placement of buildings in the game world.
    // No Burst due to Input usage and camera access.
    // Consider refactoring to separate input handling and raycasting.
    public void OnUpdate(ref SystemState state)
    {
        if(SystemAPI.TryGetSingleton<BuildingTokenComponent>(out var token))
        {
            var mousePosition = Mouse.current.position;
            var ray = Camera.main.ScreenPointToRay(mousePosition.value);
            // Define the terrain layer (example: 8)

            const uint TerrainLayer = 1 << 8;

            RaycastInput input = new RaycastInput()
            {
                Start = ray.origin,
                End = ray.origin + ray.direction * 1000f,
                Filter = new CollisionFilter()
                {
                    BelongsTo = TerrainLayer,
                    CollidesWith = TerrainLayer, // all 1s, so all layers, collide with everything
                    GroupIndex = 0
                }
            };

            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var hit = new Unity.Physics.RaycastHit();
            bool haveHit = collisionWorld.CastRay(input, out hit);
            var hitPoint = hit.Position;
            Debug.Log($"Hit Point: {hit.Position}");

            // copilot : Visualize building placement with a preview model that tracks the hitpoint

            // copilot : Store the preview entity in a singleton or static variable (outside OnUpdate)
            // static Entity previewEntity = Entity.Null;

            // copilot : If the preview entity does not exist, instantiate it from the building prefab and add a tag component (e.g., BuildingPreviewTag)
            if (!SystemAPI.HasSingleton<BuildingPreviewTag>())
            {
                var entityManager = state.EntityManager;
                var preview = entityManager.Instantiate(SystemAPI.GetSingletonBuffer<BuildingPrefabBuffer>()[(int)token.Id].Prefab);
                entityManager.AddComponent<BuildingPreviewTag>(preview);
                // Set the preview entity's physics layer (e.g., layer 9 for "Preview")
                if (entityManager.HasComponent<PhysicsCollider>(preview))
                {
                    entityManager.RemoveComponent<PhysicsCollider>(preview);
                }

                // Optionally set a transparent material here
                // Store the preview entity as a singleton for tracking
                var previewTagEntity = entityManager.CreateEntity(typeof(BuildingPreviewTagSingleton));
                entityManager.SetComponentData(previewTagEntity, new BuildingPreviewTagSingleton { PreviewEntity = preview });
            }

            // Set up a filter that collides with everything except terrain
            var filter = new CollisionFilter
            {
                BelongsTo = ~TerrainLayer, // This entity belongs to all layers except terrain
                CollidesWith = ~TerrainLayer, // This entity collides with all layers except terrain
            };

            // Physics overlap check for obstacles/buildings, ignoring terrain
            bool isValidPlacement = true;

            // Get bounds from prefab (see previous answer for BuildingBounds)
            var prefabEntity = SystemAPI.GetSingletonBuffer<BuildingPrefabBuffer>()[(int)token.Id].Prefab;
            var bounds = state.EntityManager.GetComponentData<BuildingBounds>(prefabEntity);
            float3 center = new float3(hitPoint.x, hitPoint.y + 1, hitPoint.z) + bounds.CenterOffset;
            float3 halfExtents = bounds.HalfExtents;

#if false
            // Visualize the bounds for debugging
            {
                float3 c = center;
                float3 e = halfExtents;

                // 8 corners of the box
                float3[] corners = new float3[8];
                corners[0] = c + new float3(-e.x, -e.y, -e.z);
                corners[1] = c + new float3(-e.x, -e.y, e.z);
                corners[2] = c + new float3(-e.x, e.y, -e.z);
                corners[3] = c + new float3(-e.x, e.y, e.z);
                corners[4] = c + new float3(e.x, -e.y, -e.z);
                corners[5] = c + new float3(e.x, -e.y, e.z);
                corners[6] = c + new float3(e.x, e.y, -e.z);
                corners[7] = c + new float3(e.x, e.y, e.z);

                // Draw box edges
                Debug.DrawLine((Vector3)corners[0], (Vector3)corners[1], Color.green, 1);
                Debug.DrawLine((Vector3)corners[0], (Vector3)corners[2], Color.green, 1);
                Debug.DrawLine((Vector3)corners[0], (Vector3)corners[4], Color.green, 1);
                Debug.DrawLine((Vector3)corners[1], (Vector3)corners[3], Color.green, 1);
                Debug.DrawLine((Vector3)corners[1], (Vector3)corners[5], Color.green, 1);
                Debug.DrawLine((Vector3)corners[2], (Vector3)corners[3], Color.green, 1);
                Debug.DrawLine((Vector3)corners[2], (Vector3)corners[6], Color.green, 1);
                Debug.DrawLine((Vector3)corners[3], (Vector3)corners[7], Color.green, 1);
                Debug.DrawLine((Vector3)corners[4], (Vector3)corners[5], Color.green, 1);
                Debug.DrawLine((Vector3)corners[4], (Vector3)corners[6], Color.green, 1);
                Debug.DrawLine((Vector3)corners[5], (Vector3)corners[7], Color.green, 1);
                Debug.DrawLine((Vector3)corners[6], (Vector3)corners[7], Color.green, 1);
            }
#endif
            Debug.DrawLine(center, (Vector3)center + new Vector3(10,0), Color.red);

            var hits = new NativeList<DistanceHit>(Allocator.Temp);
            if (collisionWorld.OverlapBox(center, quaternion.identity, halfExtents, ref hits, filter) && hits.Length > 0)
            {
                Debug.Log("Invalid placement: Overlapping with another object.");
                foreach (var distanceHit in hits)
                {
                    Debug.Log($"Overlapping with entity: {distanceHit.Entity}");
                }

                isValidPlacement = false;
            }
            hits.Dispose();

            var previewTag = SystemAPI.GetSingleton<BuildingPreviewTagSingleton>();
            // copilot : Update the preview entity's position to follow the hitPoint
            if (SystemAPI.HasSingleton<BuildingPreviewTagSingleton>())
            {
                var entityManager = state.EntityManager;
                var buffer = SystemAPI.GetBuffer<BuildingChild>(previewTag.PreviewEntity);
                if (entityManager.Exists(previewTag.PreviewEntity))
                {
                    if (!isValidPlacement)
                    {
                        foreach (var child in buffer)
                        {
                            entityManager.SetComponentData(child.Value, new BuildingColor { Value = new float4(1, 0, 0, 0.5f) });
                        }

                    }
                    else
                    {
                        foreach (var child in buffer)
                        {
                            entityManager.SetComponentData(child.Value, new BuildingColor { Value = new float4(0, 1, 0, 0.5f) });
                        }
                    }

                    var previewTransform = entityManager.GetComponentData<LocalTransform>(previewTag.PreviewEntity);
                    previewTransform.Position = new float3(hitPoint.x, hitPoint.y + 1, hitPoint.z);
                    entityManager.SetComponentData(previewTag.PreviewEntity, previewTransform);
                }
            }

            if (!isValidPlacement)
            {

                return;
            }

            // copilot : On placement, remove or destroy the preview entity
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                var ecb = new EntityCommandBuffer(Allocator.Temp);
                try
                {
                     // Place the building at the hitPoint
                    var buildingEntity = state.EntityManager.Instantiate(prefabEntity);
                    ecb.AddComponent(buildingEntity, new Building
                    {
                        OwnerPlayerId = SkirmishUIController.OwnerPlayer // Assuming single player for now
                    });

                    ecb.AddComponent(buildingEntity, new LocalTransform 
                    { 
                        Position = new float3(hitPoint.x, hitPoint.y + 1, hitPoint.z),
                        Scale = 1f,
                        Rotation = quaternion.identity
                    });

                    ecb.AddComponent(buildingEntity, new PathfindingGridObstacleComponent
                    {
                        BoundsSize = new float2(bounds.HalfExtents.x * 2f, bounds.HalfExtents.z * 2f),
                        CenterOffset = new float2(bounds.CenterOffset.x, bounds.CenterOffset.z)
                    });

                    if (SystemAPI.HasComponent<SpawnPositionComponent>(buildingEntity))
                    {
                        var spawnPositionComponent = SystemAPI.GetComponentRW<SpawnPositionComponent>(buildingEntity);
                        spawnPositionComponent.ValueRW.Value = spawnPositionComponent.ValueRO.Value + new float3(hitPoint.x, hitPoint.y, hitPoint.z);
                    }

                    switch (token.Id)
                    {
                        case BuildingType.Barracks:
                            ecb.AddComponent<BarracksBuildingTag>(buildingEntity);
                            break;
                            // Add cases for other building types as needed
                    }

                    // Create notification through ECB
                    var notificationEntity = ecb.CreateEntity();
                    ecb.AddComponent(notificationEntity, new BuildingPlacedNotification
                    {
                        BuildingId = token.Id
                    });

                    // Remove token through ECB
                    ecb.DestroyEntity(SystemAPI.GetSingletonEntity<BuildingTokenComponent>());

                    // Clean up preview entities through ECB
                    if (SystemAPI.HasSingleton<BuildingPreviewTagSingleton>())
                    {
                        ecb.DestroyEntity(previewTag.PreviewEntity);
                        ecb.DestroyEntity(SystemAPI.GetSingletonEntity<BuildingPreviewTagSingleton>());
                    }
                    
                    // Execute all the commands
                    ecb.Playback(state.EntityManager);
                    Debug.Log("Building placed and token removed.");
                }
                finally
                {
                    ecb.Dispose();
                }
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

[BurstCompile]
public struct WorldRaycastJob : IJob
{
    public RaycastInput RayInput;
    [ReadOnly] public CollisionWorld CollisionWorld;
    [ReadOnly] public bool IgnoreTriggers;
    [ReadOnly] public bool IgnoreStatic;

    public NativeReference<float3> HitPointRef;

    public void Execute()
    {
        var terrainPositionCollector = new TerrainPositionCollector();

        if (CollisionWorld.CastRay(RayInput, ref terrainPositionCollector))
        {
            HitPointRef.Value = terrainPositionCollector.Position;
        }
    }
}

// copilot : Tag component to identify the preview entity
public struct BuildingPreviewTag : IComponentData { }

// copilot : Singleton component to track the preview entity
public struct BuildingPreviewTagSingleton : IComponentData
{
    public Entity PreviewEntity;
}
