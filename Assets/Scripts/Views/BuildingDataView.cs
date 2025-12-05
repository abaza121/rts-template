using CrossCut.Gameplay.Components;
using System;
using UnityEngine.UIElements;

[Serializable]
public class BuildingDataView
{
    public BuildingDataSO Data;
    public float Progress;
    public bool IsBuilding;
    public StyleEnum<DisplayStyle> ShowInQueueCount;
    public StyleEnum<DisplayStyle> ShowReadyLabel;
    public bool IsReadyForPlacement;
    public int InQueue;
    public BuildingType Id;
}
