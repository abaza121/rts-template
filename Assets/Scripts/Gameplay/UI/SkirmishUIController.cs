using CrossCut.Gameplay.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public class SkirmishUIController : MonoBehaviour
{
    public const int OwnerPlayer = 0; // Assuming single player for now
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _itemTemplate;
    [SerializeField] private BuildingConfigSO _buildingData;
    [SerializeField] private BuildingConfigSO _infantryData;

    private Dictionary<BuildingType, BuildingDataView> _viewMap = new();
    private PlayerDataView _playerDataView;
    private EntityQuery _buildingDataQueueQuery;
    private EntityQuery _buildingTokenQuery;
    private EntityQuery _buildingPlacedNotificationQuery;
    private EntityQuery _creditQuery;
    private EntityQuery _recruitedNotificationQuery;

    private bool _isInfantryEnabled = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var building in _buildingData.Buildings)
        {
            var instance = _itemTemplate.Instantiate();
            var buildingDataView = new BuildingDataView { Id = building.BuildingType, Data = building };
            buildingDataView.IsBuilding = true;
            Debug.Log($"Adding building {building.name} with ID {buildingDataView.Id}");
            _viewMap.Add(buildingDataView.Id, buildingDataView);
            buildingDataView.Data = building;
            instance.Q("ItemImage").dataSource = buildingDataView;
            instance.Q("ItemImage").RegisterCallback<ClickEvent>(evt => {
                var dataView = evt.currentTarget as VisualElement;
                var data = dataView.dataSource as BuildingDataView;
                Debug.Log($"Clicked on {data.Data.name}");
                OnItemPressed(buildingDataView);
            });

            _uiDocument.rootVisualElement.Q("SidePanelInstance").Q("BaseItemsParent").Add(instance);
        }
        _playerDataView = new PlayerDataView();
        _uiDocument.rootVisualElement.Q("SidePanelInstance").dataSource = _playerDataView; // Need to create a player data view that holds credits.

        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        _creditQuery = entityManager.CreateEntityQuery(typeof(CreditComponent), typeof(LocalPlayerTag));
        _buildingDataQueueQuery = entityManager.CreateEntityQuery(typeof(BuildingDataQueueEntity));
        _buildingTokenQuery = entityManager.CreateEntityQuery(typeof(BuildingTokenComponent));
        _buildingPlacedNotificationQuery = entityManager.CreateEntityQuery(typeof(BuildingPlacedNotification));
        _recruitedNotificationQuery = entityManager.CreateEntityQuery(typeof(RecruitedNotification));
    }

    void Update()
    {
        var entityArray = _buildingDataQueueQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        var dataArray = _buildingDataQueueQuery.ToComponentDataArray<BuildingDataQueueEntity>(Unity.Collections.Allocator.Temp);

        for (int i = 0; i < dataArray.Length; i++)
        {
            Debug.Log($"Processing building queue entity {entityArray[i]} with data {dataArray[i]}");
            var data = dataArray[i];
            if (_viewMap.TryGetValue(data.Id, out var view))
            {
                view.Progress = (1-(data.RemainingCredit/view.Data.Cost))*125;
                view.InQueue = data.InQueue;
                // Optionally update UI elements here
                if (data.InQueue == 0)
                {
                    view.Progress = 0;
                    if(view.IsBuilding)
                    {
                        view.IsReadyForPlacement = true;
                    }
                }
            }
        }

        // Consume placed building notifications
        ConsumeRecruitedNotification();
        ConsumeBuildingPlacedNotification();
        UpdateLocalCreditsView();

        entityArray.Dispose();
        dataArray.Dispose();
    }

    void ConsumeRecruitedNotification()
    {
        var dataArray = _recruitedNotificationQuery.ToComponentDataArray<RecruitedNotification>(Unity.Collections.Allocator.Temp);
        var entityArray = _recruitedNotificationQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

        for (int i = 0; i < dataArray.Length; i++)
        {
            var data = dataArray[i];
            if(data.PlayerId != OwnerPlayer)
            {
                continue; // Ignore notifications not for the local player
            }

            if (_viewMap.TryGetValue(data.BuildingType, out var view))
            {
                view.Progress = 0;
                if (view.IsBuilding)
                {
                    view.IsReadyForPlacement = true;
                    //TODO : Change architecture to avoid this, create a notification for building done and consume it here.
                    World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(entityArray[i]);
                }
            }
        }

        dataArray.Dispose();
    }

    void ConsumeBuildingPlacedNotification()
    {
        var placedBuildingsData = _buildingPlacedNotificationQuery.ToComponentDataArray<BuildingPlacedNotification>(Unity.Collections.Allocator.Temp);
        var placedBuildingEntities = _buildingPlacedNotificationQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

        for (int i = 0; i < placedBuildingsData.Length; i++)
        {
            var data = placedBuildingsData[i];
            if (_viewMap.TryGetValue(data.BuildingId, out var view))
            {
                Debug.Log($"Building {data.BuildingId} placed, updating view.");
                view.IsReadyForPlacement = false;
                view.Progress = 0;
            }

            if(data.BuildingId == (int)BuildingType.Barracks && !_isInfantryEnabled) // Assuming infantry has ID 1 for now
            {
                _isInfantryEnabled = true;
                foreach (var infantry in _infantryData.Buildings)
                {
                    var instance = _itemTemplate.Instantiate();
                    var infantryDataView = new BuildingDataView { Id = infantry.BuildingType, Data = infantry }; // Use a different ID range for infantry
                    Debug.Log($"Adding infantry {infantry.name} with ID {infantryDataView.Id}");
                    _viewMap.Add(infantryDataView.Id, infantryDataView);
                    infantryDataView.Data = infantry;
                    instance.Q("ItemImage").dataSource = infantryDataView;
                    instance.Q("ItemImage").RegisterCallback<ClickEvent>(evt => {
                        var dataView = evt.currentTarget as VisualElement;
                        var data = dataView.dataSource as BuildingDataView;
                        Debug.Log($"Clicked on {data.Data.name}");
                        OnInfantryItemPressed(infantryDataView);
                    });
                    _uiDocument.rootVisualElement.Q("SidePanelInstance").Q("InfantryItemsParent").Add(instance);
                }
            }

            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(placedBuildingEntities[i]);
        }

        placedBuildingEntities.Dispose();
        placedBuildingsData.Dispose();
    }

    void UpdateLocalCreditsView()
    {
        var creditComponent = _creditQuery.GetSingleton<CreditComponent>();
        _playerDataView.Credits = creditComponent.Credits;
    }

    void OnInfantryItemPressed(BuildingDataView dataView)
    {
        StartBuilding(dataView);
    }

    void OnItemPressed(BuildingDataView dataView)
    { 
        if(dataView.IsReadyForPlacement)
        {
            // Place the building
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;
            var tokenArray = _buildingTokenQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            if(tokenArray.Length > 0)
            {
                var tokenEntity = tokenArray[0];
                entityManager.SetComponentData(tokenEntity, new BuildingTokenComponent
                {
                    Id = dataView.Id
                });
            }
            else
            {
                var tokenEntity = entityManager.CreateEntity(typeof(BuildingTokenComponent));
                entityManager.SetComponentData(tokenEntity, new BuildingTokenComponent
                {
                    Id = dataView.Id
                });
            }

            tokenArray.Dispose();
        }
        else
        {
            StartBuilding(dataView);
        }
    }

    void StartBuilding(BuildingDataView dataView)
    {
        // Create a new building entity
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        var dataArray = _buildingDataQueueQuery.ToComponentDataArray<BuildingDataQueueEntity>(Unity.Collections.Allocator.Temp);

        for (int i = 0; i < dataArray.Length; i++)
        {
            if (dataView.Id == dataArray[i].Id)
            {
                // Fix: Copy struct, modify, then use SetComponentData to update
                var updatedData = dataArray[i];
                updatedData.InQueue += 1;
                entityManager.SetComponentData(_buildingDataQueueQuery.ToEntityArray(Unity.Collections.Allocator.Temp)[i], updatedData);
                dataArray.Dispose();
                return;
            }
        }

        dataArray.Dispose();

        var entity = entityManager.CreateEntity(
            typeof(BuildingDataQueueEntity)
        // Add other required components here
        );

        // Example: Set BuildingData values (customize as needed)
        entityManager.SetComponentData(entity, new BuildingDataQueueEntity
        {
            Cost = dataView.Data.Cost,
            RemainingCredit = dataView.Data.Cost,
            Id = dataView.Id,
            InQueue = 1,
            OwnerPlayerId = OwnerPlayer, // Assuming single player for now
            IsBuilding = dataView.IsBuilding
        });
    }
}
