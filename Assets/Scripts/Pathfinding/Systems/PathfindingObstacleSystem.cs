using CrossCut.Pathfinding.Components;
using CrossCut.Pathfinding.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(PathfindingJobSystem))] // Run before we calculate paths
[BurstCompile]
public partial struct PathfinindgGridObstacleSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PathfindingGridComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gridEntity = SystemAPI.GetSingletonEntity<PathfindingGridComponent>();
        var gridData = SystemAPI.GetComponent<PathfindingGridComponent>(gridEntity);
        var gridBuffer = SystemAPI.GetBuffer<GridPathNodeBuffer>(gridEntity);


        // Assuming you manage your array via a Manager or Blob.
        // For this example, we fetch the Raw pointer to the nodes again.
        NativeArray<PathNode> nodes = gridBuffer.Reinterpret<PathNode>().AsNativeArray();

        // 1. RESET JOB: 
        // Reset all nodes to Walkable first (Clear previous frame's dynamic obstacles)
        // If you have a static map (walls that never move), you would Copy that 
        // static array into 'nodes' here instead of just setting everything to true.
        var resetJob = new ResetGridJob
        {
            gridNodes = nodes
        };
        state.Dependency = resetJob.Schedule(nodes.Length, 64, state.Dependency);

        // 2. OBSTACLE JOB:
        // Apply all current obstacles to the grid
        var applyJob = new ApplyObstaclesJob
        {
            gridNodes = nodes,
            gridWidth = gridData.GridWidth,
            gridHeight = gridData.GridHeight,
            cellSize = gridData.CellSize,
            gridOrigin = gridData.GridOrigin
        };

        state.Dependency = applyJob.ScheduleParallel(state.Dependency);
    }
}

// === Job 1: Reset the Grid ===
[BurstCompile]
partial struct ResetGridJob : IJobParallelFor
{
    public NativeArray<PathNode> gridNodes;

    public void Execute(int index)
    {
        var node = gridNodes[index];
        node.isWalkable = true; // Or set to your base terrain state
        gridNodes[index] = node;
    }
}

// === Job 2: Apply Obstacles ===
[BurstCompile]
partial struct ApplyObstaclesJob : IJobEntity
{
    [NativeDisableParallelForRestriction] // Safe because multiple objects might write 'false' to the same node (idempotent)
    public NativeArray<PathNode> gridNodes;

    public int gridWidth;
    public int gridHeight;
    public float cellSize;
    public float3 gridOrigin;

    public void Execute(in LocalTransform transform, in PathfindingGridObstacleComponent obstacle)
    {
        // 1. Calculate World Bounds (AABB)
        float2 minPos = new float2(transform.Position.x + obstacle.CenterOffset.x, transform.Position.z + obstacle.CenterOffset.y) - (obstacle.BoundsSize * 0.5f);
        float2 maxPos = new float2(transform.Position.x + obstacle.CenterOffset.x, transform.Position.z + obstacle.CenterOffset.y) + (obstacle.BoundsSize * 0.5f);

        // 2. Convert World Bounds to Grid Coordinates
        int minX = (int)math.floor((minPos.x - gridOrigin.x) / cellSize);
        int minY = (int)math.floor((minPos.y - gridOrigin.z) / cellSize);
        int maxX = (int)math.floor((maxPos.x - gridOrigin.x) / cellSize);
        int maxY = (int)math.floor((maxPos.y - gridOrigin.z) / cellSize);

        // 3. Clamp to Grid Size (Prevent out of bounds errors)
        minX = math.clamp(minX, 0, gridWidth - 1);
        maxX = math.clamp(maxX, 0, gridWidth - 1);
        minY = math.clamp(minY, 0, gridHeight - 1);
        maxY = math.clamp(maxY, 0, gridHeight - 1);

        // 4. Iterate over the affected area and mark unwalkable
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int index = x * gridHeight + y;

                // We modify the struct copy and write it back
                var node = gridNodes[index];
                node.isWalkable = false;
                gridNodes[index] = node;
            }
        }
    }
}