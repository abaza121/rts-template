using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
partial struct CreditSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        // Cache player credits by OwnerPlayerId for efficient lookup
        int playerCount = SystemAPI.QueryBuilder().WithAll<PlayerComponent>().Build().CalculateEntityCount();
        NativeArray<int> playerIds = new NativeArray<int>(playerCount, Allocator.Temp);

        foreach (var buildingData in SystemAPI.Query<RefRW<BuildingDataQueueEntity>>())
        {
            var bd = buildingData.ValueRO;
            if (bd.InQueue <= 0 || bd.OwnerPlayerId < 0 || bd.RemainingCredit == 0)
                continue;

            // Calculate credit cost per second
            playerIds[buildingData.ValueRO.PlayerId] += 1;
            buildingData.ValueRW.RemainingCredit -= 1;
        }


        // Find the player entity with matching OwnerPlayerId
        foreach ((var creditComponent, var playerComponent) in SystemAPI.Query<RefRW<CreditComponent>, RefRO<PlayerComponent>>())
        {
            creditComponent.ValueRW.Credits -= playerIds[playerComponent.ValueRO.PlayerId];
        }

        playerIds.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
