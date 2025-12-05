using Unity.Burst;
using Unity.Entities;
using CrossCut.Pathfinding.Components;

partial struct CommandProcessorSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SelectedTag>();
    }

    
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var gridSingleton = SystemAPI.GetSingleton<PathfindingGridComponent>();

        foreach (var (movementComponent, movementCommandComponent, entity) in SystemAPI.Query<RefRW<MovementComponent>, RefRO<MovementCommandComponent>>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new PathfindingRequestComponent
            {
                beginPosition = PathfindingRequestComponent.WorldToGridPosition(movementComponent.ValueRO.CurrentLocation, gridSingleton.CellSize, gridSingleton.GridOrigin),
                endPosition = PathfindingRequestComponent.WorldToGridPosition(movementCommandComponent.ValueRO.TargetPosition,gridSingleton.CellSize, gridSingleton.GridOrigin)
            });

            ecb.RemoveComponent<MovementCommandComponent>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
