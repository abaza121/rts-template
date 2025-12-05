using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace CrossCut.Pathfinding.Authoring
{
    /// <summary>
    /// Represents an authoring component for defining obstacle physics shapes in a Unity scene.
    /// </summary>
    /// <remarks>Attach this component to a GameObject to specify its physical shape using the associated
    /// PhysicsShapeAuthoring. This is typically used in physics-based simulations or games to configure collision and
    /// interaction properties for obstacles.</remarks>
    class ObstacleAuthoring : MonoBehaviour
    {
        public PhysicsShapeAuthoring PhysicsShapeAuthoring;
    }

    /// <summary>
    /// Bakes obstacle authoring data into an entity by adding a component that defines grid-based pathfinding obstacle
    /// properties.
    /// </summary>
    /// <remarks>This baker extracts collider information from the provided authoring object to determine the
    /// obstacle's bounds and center offset. The resulting component enables grid-based pathfinding systems to recognize
    /// and process obstacles in the scene. This class is typically used within the Unity Entities baking workflow to
    /// convert authoring data into runtime components.</remarks>
    class ObstacleAuthoringBaker : Baker<ObstacleAuthoring>
    {
        public override void Bake(ObstacleAuthoring authoring)
        {
            // Retrieve collider information from the authoring component
            var collider = authoring.PhysicsShapeAuthoring;
            var halfExtents = collider != null ? new float3(collider.GetBoxProperties().Size * 0.5f) : new float3(0.5f, 0.5f, 0.5f);
            var centerOffset = collider != null ? new float3(collider.GetBoxProperties().Center) : new float3(0.5f, 0.5f, 0.5f);

            // Create an entity and add the PathfindingGridObstacleComponent with calculated properties
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PathfindingGridObstacleComponent
            {
                BoundsSize = new float2(halfExtents.x * 2f, halfExtents.z * 2f),
                CenterOffset = new float2(centerOffset.x, centerOffset.z)
            });
        }
    }
}
