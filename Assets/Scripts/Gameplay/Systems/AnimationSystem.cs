using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct AnimationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AnimationComponent>();
        state.RequireForUpdate<MovementComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var isMovingId = Animator.StringToHash("IsWalking");

        foreach (var (animation, movement) in SystemAPI.Query<AnimationComponent, MovementComponent>())
        {
            animation.animationObject.transform.localRotation = movement.CurrentDirection;
            animation.animationObject.transform.localPosition = movement.CurrentLocation;
            animation.animationObject.gameObject.GetComponent<Animator>().SetBool(isMovingId, movement.IsMoving);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
