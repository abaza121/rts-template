using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class RecruitableEntityPrefabValidator : MonoBehaviour
{
    void OnValidate()
    {
        // Ensure the GameObject is tagged properly
        if (!CompareTag("EntityPrefab"))
        {
            gameObject.tag = "EntityPrefab";
            Debug.Log($"Added 'EntityPrefab' tag to {gameObject.name}");
        }

        // Verify essential components
        if (!TryGetComponent<Transform>(out _))
        {
            Debug.LogError($"Entity prefab {gameObject.name} is missing required Transform component!");
        }
    }

    void Awake()
    {
        // This prefab should never be instantiated directly
        if (Application.isPlaying)
        {
            Debug.LogError($"Entity prefab {gameObject.name} should not be instantiated directly!");
            Destroy(gameObject);
        }
    }
}