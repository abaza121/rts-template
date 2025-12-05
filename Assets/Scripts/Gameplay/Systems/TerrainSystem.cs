using CrossCut.Pathfinding.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace CrossCut.Gameplay.Systems
{
    public partial struct TerrainSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entity = SystemAPI.GetSingletonEntity<TerrainComponent>();
            var buffer = SystemAPI.GetBuffer<GridPathNodeBuffer>(entity);
            var terrainComponent = SystemAPI.GetSingleton<TerrainComponent>();
            var pathfindingGridComponent = SystemAPI.GetSingleton<PathfindingGridComponent>();

            int width = terrainComponent.GridWidth;
            int height = terrainComponent.GridHeight;
            int count = width * height;

            // Instantiate all grid cells from the prefab
            var instances = state.EntityManager.Instantiate(terrainComponent.Prefab, count, Allocator.Temp);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = x * height + y; // row-major order
                    var cellTransform = SystemAPI.GetComponentRW<LocalTransform>(instances[index]);
                    var worldPos = new Unity.Mathematics.float3(x * terrainComponent.Scale, 0, y * terrainComponent.Scale);
                    cellTransform.ValueRW.Position = worldPos;
                    cellTransform.ValueRW.Scale = terrainComponent.Scale;
                    buffer.Insert(index, new GridPathNodeBuffer
                    {
                        Value = new PathNode
                        {
                            worldPosition = worldPos,
                            isWalkable = true
                        }
                    });
                }
            }

            instances.Dispose();
            state.Enabled = false;
        }
    }
}
