using CrossCut.Pathfinding.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CrossCut.Gameplay
{
    public class TerrainAuthoring : MonoBehaviour
    {
        [SerializeField] public int Scale = 1;
        [SerializeField] public int GridWidth = 10;
        [SerializeField] public int GridHeight = 10;
        [SerializeField] public GameObject QuadPrefab;

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

                AddComponent(entity, new PathfindingGridComponent
                {
                    GridWidth = authoring.GridWidth,
                    GridHeight = authoring.GridHeight,
                    CellSize = authoring.Scale,
                    GridOrigin = new float3(-0.5f * authoring.Scale, 0, -0.5f * authoring.Scale)
                });

                AddComponent(entity, new PathfindingSettingsComponent()
                {
                    allowDiagonalMovement = true,
                    moveCost = 1
                });

                AddBuffer<GridPathNodeBuffer>(entity);
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
