using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MovementSystem : ISystem
{
    private Random random;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MovementComponent>();
        random = Random.CreateFromIndex((uint)SystemAPI.Time.ElapsedTime);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        float3 spawnPosition = float3.zero;
        bool spawnPositionSet = false;
        foreach (var (movement,transform, entity) in SystemAPI.Query<MovementComponent, LocalTransform>().WithEntityAccess())
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
                var newTransform = transform;
                newTransform.Position = newMovement.CurrentLocation;
                ecb.SetComponent(entity, newMovement);
                ecb.SetComponent(entity, newTransform);
            }
            else
            {
                // move normally.
                var newMovement = movement;
                if(movement.Destination.x != 0 || movement.Destination.y != 0 || movement.Destination.z != 0)
                {
                    MoveTowards(ref newMovement, movement.Destination, SystemAPI.Time.DeltaTime);
                    newMovement.IsMoving = true;
                }
                else
                {
                    newMovement.IsMoving = false;
                }

                var newTransform = transform;
                newTransform.Position = newMovement.CurrentLocation;
                ecb.SetComponent(entity, newMovement);
                ecb.SetComponent(entity, newTransform);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void MoveTowards(ref MovementComponent movement, float3 targetPosition, float deltaTime)
    {
        if(math.distancesq(movement.CurrentLocation, targetPosition) < 0.1f)
        {
            movement.CurrentLocation = targetPosition;
            movement.Destination = float3.zero; // Clear target
            return;
        }

        float3 direction = math.normalize(targetPosition - movement.CurrentLocation);
        movement.CurrentLocation += direction * movement.Speed * deltaTime;
        movement.CurrentDirection = quaternion.LookRotation(direction, math.up());
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
