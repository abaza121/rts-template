using CrossCut.Pathfinding.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

partial struct SelectableSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    public void OnUpdate(ref SystemState state)
    {
        var mousePosition = Mouse.current.position;
        var ray = Camera.main.ScreenPointToRay(mousePosition.value);
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        // Define the terrain layer (example: 8)

        const uint UnitLayer = 1 << 3;

        RaycastInput input = new RaycastInput()
        {
            Start = ray.origin,
            End = ray.origin + ray.direction * 1000f,
            Filter = new CollisionFilter()
            {
                BelongsTo = UnitLayer,
                CollidesWith = UnitLayer, // all 1s, so all layers, collide with everything
                GroupIndex = 0
            }
        };

        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        // if right mouse button is pressed, add a command entity to the command queue of the selected entity.
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var hit = new Unity.Physics.RaycastHit();

            // Clear previous selection
            foreach (var (selectedTag, entity) in SystemAPI.Query<SelectedTag>().WithEntityAccess())
            {
                ecb.RemoveComponent<SelectedTag>(entity);
            }

            if (!collisionWorld.CastRay(input, out hit))
            {
                return;
            }

            Debug.Log("Hit Entity: " + hit.Entity);

            ecb.AddComponent<SelectedTag>(hit.Entity);

            // hit.Entity
            // Add component to command
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            const uint TerrainLayer = 1 << 8;

            RaycastInput terrainInput = new RaycastInput()
            {
                Start = ray.origin,
                End = ray.origin + ray.direction * 1000f,
                Filter = new CollisionFilter()
                {
                    BelongsTo = TerrainLayer,
                    CollidesWith = TerrainLayer, // all 1s, so all layers, collide with everything except units
                    GroupIndex = 0
                }
            };

            var terrainHit = new Unity.Physics.RaycastHit();

            if (!collisionWorld.CastRay(terrainInput, out terrainHit))
            {
                return;
            }


            foreach (var (selectedTag, entity) in SystemAPI.Query<SelectedTag>().WithEntityAccess())
            {
                // Create command entity to move selected entity to terrainHit position
                ecb.AddComponent(entity, new MovementCommandComponent
                {
                    TargetPosition = terrainHit.Position
                });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
