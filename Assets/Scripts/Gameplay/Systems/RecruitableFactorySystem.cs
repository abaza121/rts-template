using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

partial struct RecruitableFactorySystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerComponent>();
        state.RequireForUpdate<BuildingDataQueueEntity>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (buildingData, queueEntity) in SystemAPI.Query<RefRW<BuildingDataQueueEntity>>().WithEntityAccess())
        {
            if(buildingData.ValueRW.RemainingCredit <= 0 && buildingData.ValueRW.InQueue > 0)
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent(entity, new RecruitedNotification { BuildingType = buildingData.ValueRO.Id, PlayerId = buildingData.ValueRO.PlayerId });
                buildingData.ValueRW.InQueue--;
                buildingData.ValueRW.RemainingCredit = buildingData.ValueRW.Cost;
                if (buildingData.ValueRW.InQueue <= 0)
                {
                    ecb.DestroyEntity(queueEntity);
                    Debug.Log($"Destroyed Queue Entity for Building Id: {buildingData.ValueRW.Id}");
                }        
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
