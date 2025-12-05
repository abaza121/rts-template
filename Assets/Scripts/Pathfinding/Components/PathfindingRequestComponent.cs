using Unity.Entities;
using Unity.Mathematics;

namespace CrossCut.Pathfinding.Components
{
    /// <summary>
    /// Represents a pathfinding request, including the start and end positions and the status of the path search.
    /// </summary>
    /// <remarks>This component is typically used in ECS-based systems to store information about a single
    /// pathfinding query. The positions are specified in grid coordinates, and the path search status is indicated by
    /// the isPathFound field.</remarks>
    public struct PathfindingRequestComponent : IComponentData
    {
        public int2 beginPosition;
        public int2 endPosition;
        public bool isPathFound;

        /// <summary>
        /// World to grid position conversion utility.
        /// </summary>
        /// <param name="worldPosition">world position.</param>
        /// <param name="cellSize">the size of the cell.</param>
        /// <param name="gridOrigin">the origin world position of the grid.</param>
        /// <returns>the grid position in int type.</returns>
        public static int2 WorldToGridPosition(float3 worldPosition, float cellSize, float3 gridOrigin)
        {
            return new int2(
                (int)math.floor((worldPosition.x - gridOrigin.x) / cellSize),
                (int)math.floor((worldPosition.z - gridOrigin.z) / cellSize)
            );
        }
    }
}

