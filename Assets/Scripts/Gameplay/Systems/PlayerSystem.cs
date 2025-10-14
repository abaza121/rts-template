using Unity.Burst;
using Unity.Entities;

partial struct PlayerSystem : ISystem
{
    private const int playerCount = 2; // Move to managed config and create singleton entity
    private const int localHostPlayerId = 0; // Move to managed config and create singleton entity

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        for(int i = 0; i < playerCount; i++)
        {
            var entity = state.EntityManager.CreateEntity();
            if (i == localHostPlayerId)
            {
                state.EntityManager.AddComponentData(entity, new LocalPlayerTag());
            }

            state.EntityManager.AddComponentData(entity, new PlayerComponent { PlayerId = i });
            state.EntityManager.AddComponentData(entity, new CreditComponent { Credits = 25000 });
        }
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
