using Unity.Entities;
using UnityEngine;

public class AnimationComponent : IComponentData
{
    public GameObject animationObject;
    public bool IsWalking;
}
