using UnityEngine;

[CreateAssetMenu(fileName = "BuildingConfigSO", menuName = "Scriptable Objects/BuildingConfigSO")]
public class BuildingConfigSO : ScriptableObject
{
    public BuildingDataSO[] Buildings => _buildingDataSO;
    [SerializeField]
    private BuildingDataSO[] _buildingDataSO;
}
