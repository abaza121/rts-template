using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

partial struct MovementSystem : ISystem
{
    private Unity.Mathematics.Random random;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MovementComponent>();
        random = Unity.Mathematics.Random.CreateFromIndex((uint)SystemAPI.Time.ElapsedTime);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        float3 spawnPosition = float3.zero;
        bool spawnPositionSet = false;
        foreach (var (movement, entity) in SystemAPI.Query<MovementComponent>().WithEntityAccess())
        {
            if (movement.CurrentLocation.x == float.MaxValue)
            {
                // set to spawn location.
                if(!spawnPositionSet)
                {
                    foreach (var (barracks, barracksEnt) in SystemAPI.Query<Building>().WithAny<BarracksBuildingTag>().WithEntityAccess())
                    {
                        if(barracks.OwnerPlayerId != SkirmishUIController.OwnerPlayer) // Local player id
                        {
                            continue;
                        }

                        spawnPosition = SystemAPI.GetComponent<SpawnPositionComponent>(barracksEnt).Value;
                        spawnPositionSet = true;
                        break;
                    }
                }

                var newMovement = movement;
                newMovement.IsMoving = false;
                newMovement.CurrentLocation = spawnPosition;
                ecb.SetComponent(entity, newMovement);
            }
            else
            {
                // move normally.
                var newMovement = movement;
                if(movement.Destination.x != 0 || movement.Destination.y != 0 || movement.Destination.z != 0)
                {
                    MoveTowards(ref newMovement, movement.Destination, SystemAPI.Time.DeltaTime);
                }
                else
                {
                    // Move randomly
                    newMovement.Destination = newMovement.CurrentLocation + new float3(random.NextFloat(10),0, random.NextFloat(10));
                }

                newMovement.IsMoving = true;
                ecb.SetComponent(entity, newMovement);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void MoveTowards(ref MovementComponent movement, float3 targetPosition, float deltaTime)
    {
        float3 direction = math.normalize(targetPosition - movement.CurrentLocation);
        movement.CurrentLocation += direction * movement.Speed * deltaTime;
        movement.CurrentDirection = quaternion.LookRotation(direction, math.up());

        // Check if reached target
        if (math.distancesq(movement.CurrentLocation, targetPosition) < 0.1f)
        {
            movement.CurrentLocation = targetPosition;
            movement.Destination = float3.zero; // Clear target
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
