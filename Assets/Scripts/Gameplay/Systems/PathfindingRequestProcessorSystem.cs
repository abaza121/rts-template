using Unity.Burst;
using Unity.Entities;
using CrossCut.Pathfinding.Components;
using Unity.Mathematics;

partial struct PathfindingRequestProcessorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (movement, buffer, entity) in SystemAPI.Query<RefRW<MovementComponent>, DynamicBuffer<PathBuffer>>().WithEntityAccess())
        {
            if(buffer.Length == 0)
                continue;

            if (movement.ValueRW.Destination.x == 0f && movement.ValueRW.Destination.y == 0f && movement.ValueRW.Destination.z == 0f)
            {
                movement.ValueRW.Destination = buffer[0].Value;
                buffer.RemoveAt(0);
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
