using CrossCut.Gameplay.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RecruitableRepoAuthoring : MonoBehaviour
{
    public GameObject[] RecruitablePrefabs;
    public GameObject[] RecruitableEntityPrefabs;
    public BuildingType[] BuildingType;

    private void OnValidate()
    {
        if (RecruitablePrefabs?.Length != RecruitableEntityPrefabs?.Length || 
            RecruitablePrefabs?.Length != BuildingType?.Length)
        {
            Debug.LogError("All arrays must have the same length!");
            return;
        }

        for (int i = 0; i < RecruitableEntityPrefabs?.Length; i++)
        {
            var entityPrefab = RecruitableEntityPrefabs[i];
            if (entityPrefab != null && !entityPrefab.CompareTag("EntityPrefab"))
            {
                Debug.LogError($"Entity prefab '{entityPrefab.name}' must have the 'EntityPrefab' tag!");
            }
        }
    }
}

public class RecruitableRepoAuthoringBaker : Baker<RecruitableRepoAuthoring>
{
    public override void Bake(RecruitableRepoAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        var dictionary = new Dictionary<BuildingType, RecruitablePrefabWrppaer>();
        var buffer = AddBuffer<RecruitablePrefabBuffer>(entity);


        for (int i = 0; i < authoring.RecruitablePrefabs.Length; i++)
        {
            var animPrefab = authoring.RecruitablePrefabs[i];
            var entityPrefab = authoring.RecruitableEntityPrefabs[i];
            var buildingType = authoring.BuildingType[i];

            if (animPrefab == null || entityPrefab == null)
            {
                Debug.LogError($"Missing prefab for building type {buildingType}");
                continue;
            }

            // Get the entity prefab with proper transformation usage
            var prefabEntity = GetEntity(entityPrefab, TransformUsageFlags.Dynamic);
            buffer.Add(new RecruitablePrefabBuffer { Prefab = prefabEntity });

            // Debug log to track entity creation
            Debug.Log($"Baking entity prefab for {buildingType}: GameObject={entityPrefab.name}, Entity={prefabEntity.Index}");

            dictionary[buildingType] = new RecruitablePrefabWrppaer
            {
                AnimationPrefab = animPrefab,
                BufferIndex = i
            };

            // Add a buffer to track component types for debugging
            object[] componentTypes = entityPrefab.GetComponents<Component>();
            Debug.Log($"Entity prefab {entityPrefab.name} has {componentTypes.Length} components: {string.Join(", ", componentTypes)}");
        }

        // Store the dictionary as a managed component
        var repoManaged = new RecruitableRepoManaged { PrefabMap = dictionary };
        AddComponentObject(entity, repoManaged);
        AddComponent(entity, new RecruitableRepoTag());

        // Debug log the final dictionary
        foreach (var kvp in dictionary)
        {
            Debug.Log($"Added to PrefabMap: {kvp.Key} -> Entity:{kvp.Value.BufferIndex}, AnimPrefab:{kvp.Value.AnimationPrefab.name}");
        }
    }
}


public class RecruitableRepoManaged : IComponentData
{
    public Dictionary<BuildingType, RecruitablePrefabWrppaer> PrefabMap;
}

public struct RecruitablePrefabWrppaer
{
    public GameObject AnimationPrefab;
    public int BufferIndex;
}

// Buffer element for prefab entity references
public struct RecruitablePrefabBuffer : IBufferElementData
{
    public Entity Prefab;
}

public struct RecruitableRepoTag : IComponentData
{

}
