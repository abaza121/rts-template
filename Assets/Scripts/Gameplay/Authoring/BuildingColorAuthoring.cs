using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class BuildingColorAuthoring : MonoBehaviour
{

}


class BuildingColorAuthoringBaker : Baker<BuildingColorAuthoring>
{
    public override void Bake(BuildingColorAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new BuildingColor { Value = new float4(1,1,1,1) });
    }
}