using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct TerrainPositionCollector : ICollector<RaycastHit>
{
    public float3 Position;

    public bool EarlyOutOnFirstHit => true;

    public float MaxFraction => 10;

    public int NumHits => 1;

    public bool AddHit(RaycastHit hit)
    {
        Position = hit.Position;
        return true;
    }
}
