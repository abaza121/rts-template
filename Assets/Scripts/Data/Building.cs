using Unity.Entities;
using CrossCut.Pathfinding.Components;

public struct Building : IComponentData
{
    public int OwnerPlayerId;
}

public struct BarracksBuildingTag : IComponentData
{
    
}
