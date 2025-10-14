using CrossCut.Gameplay.Components;
using Unity.Entities;

public struct BuildingPlacedNotification : IComponentData
{
    public BuildingType BuildingId;
}