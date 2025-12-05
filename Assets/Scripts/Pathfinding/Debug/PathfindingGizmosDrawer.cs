using CrossCut.Pathfinding.Components;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class PathfindingGizmosDrawer : MonoBehaviour
{
    [SerializeField]
    private bool DrawGridCells = true;

    [SerializeField]
    private bool DrawWalkableOnly = false;

    [SerializeField]
    private float CellSize = 1f;

    [SerializeField]
    private Color WalkableColor = new Color(0, 1, 0, 0.3f); // Green with transparency

    [SerializeField]
    private Color ObstacleColor = new Color(1, 0, 0, 0.3f); // Red with transparency

    private World DefaultWorld => World.DefaultGameObjectInjectionWorld;

    public float VerticalLineOffset;
    public float HorizontalLineOffset;

    private void OnDrawGizmos()
    {
        if (!DrawGridCells)
            return;

        if (DefaultWorld == null || !DefaultWorld.IsCreated)
            return;

        var entityManager = DefaultWorld.EntityManager;

        // Query for the PathfindingGridComponent singleton
        var gridQuery = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<PathfindingGridComponent>(),
            ComponentType.ReadOnly<GridPathNodeBuffer>());

        using (gridQuery)
        {
            if (gridQuery.CalculateEntityCount() == 0)
                return;

            var gridEntity = gridQuery.GetSingletonEntity();
            var gridComponent = entityManager.GetComponentData<PathfindingGridComponent>(gridEntity);
            var gridBuffer = entityManager.GetBuffer<GridPathNodeBuffer>(gridEntity);

            DrawGrid(gridComponent, gridBuffer);

            // Draw any active pathfinding requests (start, end, and reconstructed path if available)
            DrawPathfindingRequests(entityManager, gridComponent, gridBuffer);
        }
    }

    private void DrawGrid(PathfindingGridComponent gridComponent, DynamicBuffer<GridPathNodeBuffer> gridBuffer)
    {
        int gridWidth = gridComponent.GridWidth;
        int gridHeight = gridComponent.GridHeight;

        // Draw grid cells
        for (int i = 0; i < gridBuffer.Length; i++)
        {
            PathNode node = gridBuffer[i].Value;

            // Skip non-walkable cells if filtering is enabled
            if (DrawWalkableOnly && !node.isWalkable)
                continue;

            Color cellColor = node.isWalkable ? WalkableColor : ObstacleColor;
            DrawCell(node.worldPosition, CellSize, cellColor);
        }

        // Draw grid lines
        DrawGridLines(gridWidth, gridHeight);
    }

    private void DrawPathfindingRequests(EntityManager entityManager, PathfindingGridComponent gridComponent, DynamicBuffer<GridPathNodeBuffer> gridBuffer)
    {
        // Query for all entities that have a PathfindingRequestComponent.
        using (var requestQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PathfindingRequestComponent>()))
        {
            int requestCount = requestQuery.CalculateEntityCount();
            if (requestCount == 0)
                return;

            // Get all entities with requests
            using (var entities = requestQuery.ToEntityArray(Allocator.Temp))
            {
                for (int ei = 0; ei < entities.Length; ei++)
                {
                    var entity = entities[ei];
                    var request = entityManager.GetComponentData<PathfindingRequestComponent>(entity);

                    // Compute indices using same indexing scheme as the pathfinder.
                    int startIndex = request.beginPosition.x * gridComponent.GridHeight + request.beginPosition.y;
                    int endIndex = request.endPosition.x * gridComponent.GridHeight + request.endPosition.y;

                    Vector3 startWorld;
                    Vector3 endWorld;

                    // Validate indices and fetch world positions from the grid buffer if valid.
                    if (startIndex >= 0 && startIndex < gridBuffer.Length)
                        startWorld = gridBuffer[startIndex].Value.worldPosition;
                    else
                        startWorld = GridPositionToWorld(request.beginPosition, gridComponent);

                    if (endIndex >= 0 && endIndex < gridBuffer.Length)
                        endWorld = gridBuffer[endIndex].Value.worldPosition;
                    else
                        endWorld = GridPositionToWorld(request.endPosition, gridComponent);

                    // Small upward offset so gizmos render above the grid cubes
                    Vector3 drawOffset = Vector3.up * 0.25f;

                    // Draw start and end markers
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(startWorld + drawOffset, Mathf.Max(0.05f, CellSize * 0.25f));
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(endWorld + drawOffset, Mathf.Max(0.05f, CellSize * 0.25f));

                    // Draw a line between start and end
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(startWorld + drawOffset, endWorld + drawOffset);

                    // If this entity has a reconstructed PathBuffer, draw it (waypoints + connecting lines)
                    if (entityManager.HasComponent<PathBuffer>(entity))
                    {
                        var pathBuffer = entityManager.GetBuffer<PathBuffer>(entity);
                        if (pathBuffer.Length > 0)
                        {
                            Gizmos.color = Color.yellow;
                            Vector3 prev = (Vector3)pathBuffer[0].Value + drawOffset;
                            // draw spheres for waypoints and lines between them
                            Gizmos.DrawSphere(prev, Mathf.Max(0.03f, CellSize * 0.15f));
                            for (int i = 1; i < pathBuffer.Length; i++)
                            {
                                Vector3 cur = (Vector3)pathBuffer[i].Value + drawOffset;
                                Gizmos.DrawSphere(cur, Mathf.Max(0.03f, CellSize * 0.15f));
                                Gizmos.DrawLine(prev, cur);
                                prev = cur;
                            }
                        }
                    }

                    // Optionally draw an indicator when the request reports path found
                    if (request.isPathFound)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(endWorld + drawOffset, Mathf.Max(0.07f, CellSize * 0.3f));
                    }
                }
            }
        }
    }

    private Vector3 GridPositionToWorld(int2 gridPos, PathfindingGridComponent gridComponent)
    {
        // Fallback conversion if direct buffer lookup fails.
        // Place cell center at (x * CellSize + CellSize/2, 0, y * CellSize + CellSize/2).
        float half = CellSize * 0.5f;
        return new Vector3(gridPos.x * CellSize + half, 0f, gridPos.y * CellSize + half);
    }

    private void DrawCell(Vector3 center, float size, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawCube(center, new Vector3(size, 0.1f, size));

        // Draw cell outline
        Gizmos.color = Color.white * 0.5f;
        float halfSize = size * 0.5f;
        Vector3[] corners = new Vector3[4]
        {
            center + new Vector3(-halfSize, 0.05f, -halfSize),
            center + new Vector3(halfSize, 0.05f, -halfSize),
            center + new Vector3(halfSize, 0.05f, halfSize),
            center + new Vector3(-halfSize, 0.05f, halfSize)
        };

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }

    private void DrawGridLines(int gridWidth, int gridHeight)
    {
        Gizmos.color = Color.gray * 0.5f;

        float width = gridWidth * CellSize;
        float height = gridHeight * CellSize;
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        // Draw vertical lines
        for (int x = 0; x <= gridWidth; x++)
        {
            float xPos = (x * CellSize) - 0.5f * CellSize;
            Vector3 start = new Vector3(xPos, 0.02f, -0.5f * CellSize);
            Vector3 end = new Vector3(xPos, 0.02f, height - 0.5f * CellSize);
            Gizmos.DrawLine(start, end);
        }

        // Draw horizontal lines
        for (int z = 0; z <= gridHeight; z++)
        {
            float zPos = (z * CellSize) - 0.5f * CellSize;
            Vector3 start = new Vector3(-0.5f * CellSize, 0.02f, zPos);
            Vector3 end = new Vector3(width - 0.5f * CellSize, 0.02f, zPos);
            Gizmos.DrawLine(start, end);
        }
    }
}
