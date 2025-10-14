using CrossCut.Gameplay.Components;
using System;

[Serializable]
public class BuildingDataView
{
    public BuildingDataSO Data;
    public float Progress;
    public bool IsBuilding;
    public bool IsReadyForPlacement;
    public int InQueue;
    public BuildingType Id;
}
