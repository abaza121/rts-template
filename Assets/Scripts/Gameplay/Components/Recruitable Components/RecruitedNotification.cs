using CrossCut.Gameplay.Components;
using Unity.Entities;

public struct RecruitedNotification : IComponentData
{
    public BuildingType BuildingType;
    public int PlayerId;
}