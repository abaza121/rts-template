using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CrossCut.Pathfinding.Components
{
    /// <summary>
    /// Represents the configuration data for a grid used in pathfinding operations.
    /// </summary>
    /// <remarks>This component defines the dimensions, cell size, and origin position of a pathfinding grid.
    /// It is typically attached to an entity to provide spatial information required for grid-based navigation
    /// algorithms.</remarks>
    public struct PathfindingGridComponent : IComponentData
    {
        public int GridWidth;
        public int GridHeight;
        public int CellSize;
        public float3 GridOrigin;
    }

    /// <summary>
    /// Represents a buffer element containing a single path node for use in grid-based pathfinding operations within an
    /// entity's dynamic buffer.
    /// </summary>
    /// <remarks>This struct is typically used with Unity's ECS dynamic buffers to store and process sequences
    /// of path nodes during pathfinding. Each instance encapsulates a single path node, allowing efficient manipulation
    /// and traversal of grid paths in parallel systems.</remarks>
    public struct GridPathNodeBuffer : IBufferElementData
    {
        public PathNode Value;
    }

    /// <summary>
    /// This struct is typically stored inside a NativeArray<PathNode> 
    /// held by a global PathfindingGridComponent.
    /// </summary>
    public struct PathNode
    {
        /// <summary>
        /// Represents the position of the node in world space coordinates, mainly used for visualization and debugging purposes.
        /// </summary>
        public float3 worldPosition;

        /// <summary>
        /// Indicates whether the object can be traversed or walked over.
        /// </summary>
        public bool isWalkable;
    }
}