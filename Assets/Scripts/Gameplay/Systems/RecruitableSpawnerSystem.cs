using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

partial struct RecruitableSpawnerSystem : ISystem
{
    private EntityQuery recruitedNotificationQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        recruitedNotificationQuery = state.GetEntityQuery(ComponentType.ReadOnly<RecruitedNotification>());
        state.RequireForUpdate<RecruitedNotification>();
        state.RequireForUpdate<RecruitableRepoTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (recruitedNotificationQuery.IsEmpty)
            return;

        var recruitableRepo = SystemAPI.GetSingletonEntity<RecruitableRepoTag>();
        var recruitableEntityBuffer = SystemAPI.GetBuffer<RecruitablePrefabBuffer>(recruitableRepo);
        var recruitableRepoManaged = state.EntityManager.GetComponentObject<RecruitableRepoManaged>(recruitableRepo);
        var entityManager = state.EntityManager;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (recruited, entity) in SystemAPI.Query<RefRO<RecruitedNotification>>().WithEntityAccess())
        {
            var buildingType = recruited.ValueRO.BuildingType;
            
            if (!recruitableRepoManaged.PrefabMap.ContainsKey(buildingType))
            {
                Debug.LogError($"Missing prefab for Building Type {buildingType}");
                continue;
            }

            var prefabWrapper = recruitableRepoManaged.PrefabMap[buildingType];

            // Create the game object first
            var gameObject = GameObject.Instantiate(prefabWrapper.AnimationPrefab);
            if (gameObject == null)
            {
                Debug.LogError($"Failed to instantiate animation prefab for {buildingType}");
                continue;
            }

            // Instantiate the entity
            var spawnedEntity = ecb.Instantiate(recruitableEntityBuffer[prefabWrapper.BufferIndex].Prefab);

            // Add components directly using EntityManager for immediate effect
            ecb.AddComponent(spawnedEntity, new AnimationComponent 
            { 
                IsWalking = false,
                animationObject = gameObject
            });

            ecb.AddComponent(spawnedEntity, new MovementComponent 
            { 
                CurrentLocation = new float3(float.MaxValue, float.MaxValue, float.MaxValue),
                Speed = 5f
            });

            // Use ECB only for cleanup
            ecb.DestroyEntity(entity);

            Debug.Log($"Successfully spawned recruitable entity {spawnedEntity} with GameObject {gameObject.name} for {buildingType}");
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}
