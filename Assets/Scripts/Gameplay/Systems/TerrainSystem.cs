using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
            var terrainComponent = SystemAPI.GetSingleton<TerrainComponent>();
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
                    cellTransform.ValueRW.Position = new Unity.Mathematics.float3(x * terrainComponent.Scale, 0, y * terrainComponent.Scale);
                }
            }

            instances.Dispose();
            state.Enabled = false;
        }
    }
}
