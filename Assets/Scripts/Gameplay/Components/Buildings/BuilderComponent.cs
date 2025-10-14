using Unity.Entities;

namespace CrossCut.Gameplay.Components
{
    public struct BuilderComponent : IComponentData
    {

    }

    public enum BuildingType
    {
        None = -1,
        Barracks = 0,
        PowerPlant = 1,
        Soldier = 100,
    }
}
