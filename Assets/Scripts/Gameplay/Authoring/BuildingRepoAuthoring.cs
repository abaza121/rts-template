using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingRepoAuthoring : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _buildingPrefabs;

    public GameObject[] BuildingPrefabs => _buildingPrefabs;
}

// Buffer element for prefab entity references
public struct BuildingPrefabBuffer : IBufferElementData
{
    public Entity Prefab;
}

public class BuildingRepoAuthoringBaker : Baker<BuildingRepoAuthoring>
{
    public override void Bake(BuildingRepoAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        var buffer = AddBuffer<BuildingPrefabBuffer>(entity);
        for (int i = 0; i < authoring.BuildingPrefabs.Length; i++)
        {
            var prefabEntity = GetEntity(authoring.BuildingPrefabs[i], TransformUsageFlags.Dynamic);
            buffer.Add(new BuildingPrefabBuffer { Prefab = prefabEntity });
        }
    }
}