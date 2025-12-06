# RTS ECS Project

## Overview
This project is a Real-Time Strategy (RTS) game template built using Unity's Entity Component System (ECS). It focuses on high-performance gameplay mechanics suitable for RTS titles.

## Key Features

### A* Pathfinding (ECS)
The project features a robust A* pathfinding algorithm implemented within the ECS architecture, allowing for efficient navigation of units across the map.

## Project Structure

The project follows a standard Unity ECS architecture, organized primarily by feature modules within `Assets/Scripts`:

*   **Gameplay** (`Assets/Scripts/Gameplay`): Core game mechanics.
    *   **Authoring**: `MonoBehaviour` scripts used to convert GameObjects into ECS Entities (baking).
    *   **Components**: `IComponentData` structs defining the data for entities (e.g., Unit stats, state).
    *   **Systems**: Logic implementations (e.g., Movement, Combat) that operate on Components.
*   **Pathfinding** (`Assets/Scripts/Pathfinding`): A standalone module for the A* pathfinding implementation.
    *   Contains its own `Components`, `Systems`, and `Authoring` scripts, keeping navigation logic decoupled from core gameplay.
*   **Views** (`Assets/Scripts/Views`): Hybrid systems or MonoBehaviours that handle visual representation and UI bridging with UI Toolkit.

## Prerequisites

*   **Unity Version**: 6000.2.7f2
*   **Key Packages**:
    *   `com.unity.entities` (1.3.14)
    *   `com.unity.physics` (1.3.14)
    *   `com.unity.netcode` (Implied by RTS nature, though not explicitly in manifest snippet, will stick to confirmed ones)
    *   `com.unity.ai.navigation` (2.0.9)
    *   `com.unity.collections` (Implied)
    *   `com.unity.jobs` (Implied)

## Controls

### Mouse
*   **Left Click**: Select a Unit.
*   **Right Click**: Move selected Unit(s) to the clicked location on the terrain.

### Keyboard
*   **WASD / Arrow Keys**: Camera Movement (Standard Unity Input System default).

## Getting Started

### Supported Scenes
*   **Skirmish**: This is currently the only implemented scene. Please use this scene for testing and gameplay.
*   *Other scenes are not yet implemented.*

### How to Play
1.  **Recruit Units**: Use the deployment menu to spawn units into the game.
2.  **Select Unit**: Left-click on a unit to select it.
3.  **Move Unit**: With a unit selected, Right-click on a location to move the unit there.
    *   *Note: Movement visualizations (e.g., target markers, path lines) are currently pending and will be added in a future update.*

### Debugging
To view the underlying pathfinding grid and debug information:
*   Enable the **PathfindingDebugger** in the Skirmish scene. This will render the pathfinding map and other debug visuals.

## Adding a Unit

In this project, you can add Units by specifying the art and 3D dimensions.

### 1. Define Unit Type
*   Add the enum for the new unit in `BuilderComponent`.

### 2. Configure 2D Art and Cost
*   Define the unit's data in a new `BuildingDataSO`.
*   Add this `BuildingDataSO` to  `DefaultInfantryConfig`:

### 3. Configure 3D Art (Repository)
*   Locate the `RecruitableRepoAuthoring` prefab/component.
*   Add a new entry for the unit entity and its corresponding GameObject animation prefab.
    *   This creates a repository of data in entities that defines how the prefab looks and what it is capable of.
<!-- //TODO: Add photo of RecruitableRepoAuthoring inspector -->

### 4. Create Unit Entity Prefab
Create a new Prefab for the unit entity:
*   **Tag**: Set to `EntityPrefab`.
*   **Required Components**:
    *   `Physics Body` (Unity Physics)
    *   `Physics Shape` (Unity Physics)
    *   `SelectableUnitAuthoring`: Enables command handling.
    *   `MovableUnitAuthoring`: Enables movement capabilities.
    *   `RecruitableEntityPrefabAuthoring`: Binds the entity to the animated GameObject.
<!-- //TODO: Add photo of Unit Entity Prefab inspector -->


## Adding a Building
In this project, you can add buildings for placement by specifying the art and 3D dimensions.

### 1. Define Building Type
*   Add the enum for the new unit in `BuilderComponent`.

### 2. Configure 2D Art and Cost
*   Define the building's data in a `BuildingDataSO`.
*   Add this `BuildingDataSO` to `DefaultBuildingConfig`.

### 3. Configure 3D Art (Repository)
*   Locate the `BuildingRepoAuthoring` game object in the Authoring prefab.
*   Add a new entry for the unit entity and its corresponding GameObject animation prefab.
    *   This creates a repository of data in entities that defines how the prefab looks and what it is capable of.

### 4. Create Building Entity Prefab
Create a new Prefab for the unit entity:
*   **Tag**: Set to `EntityPrefab`.
*   **Required Components**:
    *   `Physics Shape` (Unity Physics)
    *   `BuildingAuthoring`: Binds the entity to the PhysicsShape.
<!-- //TODO: Add photo of Unit Entity Prefab inspector -->

## Roadmap / Status

*   **Current State**: Prototype / Template.
*   **Implemented Scenes**: Skirmish (Testing ground).
*   **Pending Features**:
    *   Unit movement visualizations (path lines, target markers).
