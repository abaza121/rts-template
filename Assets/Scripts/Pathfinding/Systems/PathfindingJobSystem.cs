using CrossCut.Pathfinding.Components;
using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CrossCut.Pathfinding.Systems
{
    partial struct PathfindingJobSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Create the initial grid singleton here.
            state.RequireForUpdate<PathfindingGridComponent>();
            state.RequireForUpdate<PathfindingRequestComponent>();
            state.RequireForUpdate<PathfindingSettingsComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get the Command Buffer Singleton will be used to make structural changes in the job.
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // 1. Get the global, read-only grid data
            // Assume PathfindingGridComponent is a singleton or stored on a single entity.
            var gridEntity = SystemAPI.GetSingletonEntity<PathfindingGridComponent>();
            var gridComponent = SystemAPI.GetComponentRO<PathfindingGridComponent>(gridEntity);

            // Pass the NativeArray pointer with [ReadOnly] access.
            DynamicBuffer<GridPathNodeBuffer> globalGridBuffer = SystemAPI.GetBuffer<GridPathNodeBuffer>(gridEntity);
            NativeArray<PathNode> globalGrid = globalGridBuffer.Reinterpret<PathNode>().AsNativeArray();
            int gridWidth = gridComponent.ValueRO.GridWidth;
            int gridHeight = gridComponent.ValueRO.GridHeight;
            int cellSize = gridComponent.ValueRO.CellSize;

            // Populate job including settings from the world (if present)
            var job = new PathfindingJob
            {
                commandBuffer = ecb.AsParallelWriter(),
                globalGridNodes = globalGrid,
                gridWidth = gridWidth,
                gridHeight = gridHeight,
                cellSize = cellSize,
                gridOrigin = new float3(-0.5f*cellSize, 0,-0.5f*cellSize),
                settings = SystemAPI.HasSingleton<PathfindingSettingsComponent>() ? SystemAPI.GetSingleton<PathfindingSettingsComponent>() : default
            };

            // Schedule the job and assign the returned JobHandle to state.Dependency.
            // This is required so other systems (for example ExportPhysicsWorld) see the correct dependency chain.
            JobHandle handle = job.ScheduleParallel(state.Dependency);
            state.Dependency = handle;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }

    partial struct PathfindingJob : IJobEntity
    {
        // === 1. Global Read-Only Data ===
        [ReadOnly]
        public NativeArray<PathNode> globalGridNodes; // Only used to check .isWalkable

        public int gridWidth;
        public int gridHeight;
        public float cellSize; // Added to calculate world position accurately
        public float3 gridOrigin; // Added to calculate world position accurately
        public PathfindingSettingsComponent settings;
        public EntityCommandBuffer.ParallelWriter commandBuffer;

        private const float DIAGONAL_COST = 1.41f; // Approximate sqrt(2)

        // === 2. Job Execution ===
        public void Execute(
            Entity entity,
            [ChunkIndexInQuery] int chunkIndex,
            ref PathfindingRequestComponent pathRequest,
            ref DynamicBuffer<PathBuffer> pathBuffer)
        {
            // Path already found, skip.
            if (pathRequest.isPathFound) return;

            // 1. Validate Coordinates
            int2 startPos = pathRequest.beginPosition;
            int2 endPos = pathRequest.endPosition;

            if (!IsPositionValid(startPos) || !IsPositionValid(endPos))
            {
                commandBuffer.RemoveComponent<PathfindingRequestComponent>(chunkIndex, entity);
                return;
            }

            int startIndex = CalculateIndex(startPos.x, startPos.y);
            int endIndex = CalculateIndex(endPos.x, endPos.y);

            // If Start == End, no path needed
            if (startIndex == endIndex)
            {
                commandBuffer.RemoveComponent<PathfindingRequestComponent>(chunkIndex, entity);
                return;
            }

            // If Start or End is not walkable, fail immediately
            if (!globalGridNodes[startIndex].isWalkable || !globalGridNodes[endIndex].isWalkable)
            {
                commandBuffer.RemoveComponent<PathfindingRequestComponent>(chunkIndex, entity);
                return;
            }

            // Initialize A* Structures
            var gCosts = new NativeHashMap<int, int>(128, Allocator.Temp);
            var cameFrom = new NativeHashMap<int, int>(128, Allocator.Temp);
            var openList = new NativeHeap<NodeSorter,Min>(Allocator.Temp);
            var closedList = new NativeHashSet<int>(128, Allocator.Temp);

            // Add Starting point.
            gCosts.Add(startIndex, 0);
            int startH = CalculateHeuristic(startPos, endPos);
            openList.Insert(new NodeSorter { index = startIndex, fCost = startH, hCost = startH });

            bool found = false;

            while (openList.Count > 0)
            {
                // Get Node with lowest F Cost
                int currentIndex = openList.Pop().index;
                if (currentIndex == endIndex)
                {
                    found = true;
                    break;
                }

                // If already processed, skip.
                if (closedList.Contains(currentIndex)) continue;
                closedList.Add(currentIndex);

                // Calculate current X/Y from Index.
                int2 currentPos = IndexToPos(currentIndex);
                int currentGCost = gCosts[currentIndex];

                // Look at Neighbors (8 directions)
                for (int ny = -1; ny <= 1; ny++)
                {
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        // 1. Basic Validity Checks
                        // Skip the current node.
                        if (nx == 0 && ny == 0) continue;

                        // Check Neighbor position validity.
                        int2 neighborPos = new int2(currentPos.x + nx, currentPos.y + ny);
                        if (!IsPositionValid(neighborPos)) continue;

                        // Check that Neighbor is Walkable and not in Closed List.
                        int neighborIndex = CalculateIndex(neighborPos.x, neighborPos.y);
                        if (!globalGridNodes[neighborIndex].isWalkable) continue;
                        if (closedList.Contains(neighborIndex)) continue;

                        // 2. Corner Cutting Check (Prevents walking through squeezed walls)
                        bool isDiagonal = math.abs(nx) + math.abs(ny) == 2;
                        if (isDiagonal)
                        {
                            if (!settings.allowDiagonalMovement) continue;

                            // Check orthogonal neighbors to prevent cutting corners
                            int indexNode1 = CalculateIndex(currentPos.x + nx, currentPos.y);
                            int indexNode2 = CalculateIndex(currentPos.x, currentPos.y + ny);

                            if (!globalGridNodes[indexNode1].isWalkable || !globalGridNodes[indexNode2].isWalkable)
                                continue;
                        }

                        // 3. Calculate Costs
                        int moveCost = isDiagonal ?
                            (int)(settings.moveCost * DIAGONAL_COST) :
                            settings.moveCost;

                        int tentativeG = currentGCost + moveCost;

                        // 4. Evaluate Neighbor

                        // If we haven't visited this node or found a cheaper path
                        bool hasG = gCosts.TryGetValue(neighborIndex, out int existingG);
                        if (hasG && tentativeG > existingG)
                        {
                            continue; // Not a better path
                        }

                        // Update G Cost
                        if (hasG)
                        {
                            gCosts[neighborIndex] = tentativeG;
                        }
                        else
                        {
                            gCosts.Add(neighborIndex, tentativeG);
                        }

                        // Update Came From
                        if (cameFrom.ContainsKey(neighborIndex))
                        {
                            cameFrom[neighborIndex] = currentIndex;
                        }
                        else
                        {
                            cameFrom.Add(neighborIndex, currentIndex);
                        }

                        // Calculate H and F Costs and add to Open List.
                        int hCost = CalculateHeuristic(neighborPos, endPos);
                        int fCost = tentativeG + hCost;
                        openList.Insert(new NodeSorter { index = neighborIndex, fCost = fCost, hCost = hCost });
                    }
                }
            }


            // Reconstruct Path if found.
            if (found)
            {
                // Clear existing buffer.
                pathBuffer.Clear();
                var pathIndices = new NativeList<int>(Allocator.Temp);
                int curr = endIndex;

                while (curr != startIndex && cameFrom.ContainsKey(curr))
                {
                    pathIndices.Add(curr);
                    curr = cameFrom[curr];
                }
                // Note: We intentionally don't add StartNode to the buffer so the entity moves TO the first step immediately

                // Write to buffer in reverse (Start -> End)
                for (int i = pathIndices.Length - 1; i >= 0; i--)
                {
                    pathBuffer.Add(new PathBuffer 
                    { 
                        Value = globalGridNodes[pathIndices[i]].worldPosition 
                    });
                }

                pathIndices.Dispose();
                pathRequest.isPathFound = true;
                UnityEngine.Debug.Log($"Path found with {pathBuffer.Length} steps.");
            }

            // Cleanup
            gCosts.Dispose();
            cameFrom.Dispose();
            openList.Dispose();
            closedList.Dispose();

            commandBuffer.RemoveComponent<PathfindingRequestComponent>(chunkIndex, entity);
        }

        private bool IsPositionValid(int2 pos)
        {
            return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
        }

        private int CalculateIndex(int x, int y)
        {
            return x * gridHeight + y;
        }

        private int2 IndexToPos(int index)
        {
            return new int2(index / gridHeight, index % gridHeight);
        }

        // Heuristic: Manhattan (4-way) or Octile (8-way)
        private int CalculateHeuristic(int2 start, int2 end)
        {
            int xDist = math.abs(start.x - end.x);
            int yDist = math.abs(start.y - end.y);
            int remaining = math.abs(xDist - yDist);
            int diag = math.min(xDist, yDist);

            if (settings.allowDiagonalMovement)
                return (int)(14 * diag + 10 * remaining); // Standard Octile multiplier (1.41 * 10)
            else
                return 10 * (xDist + yDist);
        }

        // Improved Sorter with Tie-Breaking
        public struct NodeSorter : IComparable<NodeSorter>
        {
            public int index;
            public int fCost;
            public int hCost; // Add H cost for tie-breaking

            public int CompareTo(NodeSorter other)
            {
                // Primary: Lowest F Cost
                int compare = fCost.CompareTo(other.fCost);

                // Tie-Breaker: If F is same, pick the one closer to the target (Lower H)
                // This prevents the "Zig-Zag" / "Triangle" path on open ground
                if (compare == 0)
                {
                    compare = hCost.CompareTo(other.hCost);
                }
                return compare;
            }
        }
    }
}
