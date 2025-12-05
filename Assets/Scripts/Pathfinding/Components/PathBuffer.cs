using Unity.Entities;
using Unity.Mathematics;

namespace CrossCut.Pathfinding.Components
{
    // Buffer element used to return a reconstructed path as a sequence of world/grid positions.
    // Stores a single float3 waypoint per element so it can be used as a DynamicBuffer<PathBuffer>.
    public struct PathBuffer : IBufferElementData
    {
        public float3 Value;
    }
}