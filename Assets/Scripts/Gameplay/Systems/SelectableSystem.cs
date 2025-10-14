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

        // copilot : On placement, remove or destroy the preview entity
        // select entity that is under the mouse cursor.
        // if an entity is already selected, deselect it.

        // if right mouse button is pressed, add a command entity to the command queue of the selected entity.
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Add component to command
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
