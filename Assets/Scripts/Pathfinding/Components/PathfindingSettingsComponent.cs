using Unity.Entities;


namespace CrossCut.Pathfinding.Components
{
    /// <summary>
    /// Represents configuration settings for pathfinding behavior in an entity, including movement options and cost
    /// parameters.
    /// </summary>
    /// <remarks>Use this component to control how an entity navigates a grid or map, such as whether diagonal
    /// movement is permitted and the cost associated with movement. These settings influence pathfinding algorithms and
    /// can affect route selection and traversal efficiency.</remarks>
    public struct PathfindingSettingsComponent : IComponentData
    {
        /// <summary>
        /// Indicates whether diagonal movement is permitted.
        /// </summary>
        public bool allowDiagonalMovement;

        /// <summary>
        /// Gets or sets the cost associated with performing a move operation.
        /// </summary>
        public int moveCost;
    }
}
