using CrossCut.Gameplay.Components;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingDataSO", menuName = "Scriptable Objects/BuildingDataSO")]
public class BuildingDataSO : ScriptableObject
{
    [SerializeField]
    private Sprite _icon;
    [SerializeField]
    private string _name;
    public int Cost;
    public bool IsBuilding;
    public BuildingType BuildingType;
}
