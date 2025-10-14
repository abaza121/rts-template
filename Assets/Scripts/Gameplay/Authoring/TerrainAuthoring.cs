using Unity.Entities;
using UnityEngine;

namespace CrossCut.Gameplay
{
    public class TerrainAuthoring : MonoBehaviour
    {
        [SerializeField] public int Scale = 1;
        [SerializeField] public int GridWidth = 10;
        [SerializeField] public int GridHeight = 10;
        [SerializeField] public GameObject QuadPrefab; // Assign your quad prefab here

        class Baker : Baker<TerrainAuthoring>
        {
            public override void Bake(TerrainAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new TerrainComponent
                {
                    Prefab = GetEntity(authoring.QuadPrefab, TransformUsageFlags.Dynamic)
                    , Scale = authoring.Scale
                    , GridWidth = authoring.GridWidth
                    , GridHeight = authoring.GridHeight
                });
            }
        }
    }

    public struct TerrainComponent : IComponentData
    {
        public int Scale;
        public int GridWidth;
        public int GridHeight;
        public Entity Prefab;
    }
}
