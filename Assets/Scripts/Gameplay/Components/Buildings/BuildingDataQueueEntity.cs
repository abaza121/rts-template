using CrossCut.Gameplay.Components;
using System;
using Unity.Entities;
using UnityEngine;

[System.Serializable]
public partial struct BuildingDataQueueEntity : IComponentData
{
    public float RemainingCredit;
    public float TimeToBuild;
    public int Cost;
    public int PlayerId;
    public BuildingType Id;
    public int InQueue;
    public int OwnerPlayerId;
    public bool IsBuilding;
}
