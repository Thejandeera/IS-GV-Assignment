# Tropical Reserve Rescue

A Unity-based 3D rescue simulation game set inside a Sri Lankan eco-tourism wildlife reserve inspired by places such as Sinharaja Forest Reserve. The game combines real-time player interaction, physics-based obstacle clearing, custom AI pathfinding, graph formulation, and visual debugging.

## Project Summary

A severe storm has struck a tropical wildlife reserve. Mudslides and fallen trees have blocked the jungle routes, leaving hikers and endangered animals stranded inside the reserve. Autonomous rescue drones are deployed to locate victims and guide them back to the rescue base.

The drones use intelligent pathfinding to move through the reserve, but fallen logs and other obstacles block their routes. The player controls a monster truck and physically clears these obstacles by ramming or pulling logs away. Once a blocked path is opened, the AI updates its route dynamically and continues the rescue mission.

## Game Concept

Tropical Reserve Rescue is designed around the connection between the physical game world and a mathematical graph used by AI agents. The player does not directly control the drone. Instead, the player changes the environment, and the drone reacts intelligently.

The main gameplay loop is:

1. A rescue drone searches for a stranded target or rescue beacon.
2. The drone calculates a path through the jungle using AI search.
3. Fallen logs or terrain obstacles may block the best route.
4. The player drives a monster truck to remove obstacles.
5. The graph updates when the environment changes.
6. The drone recalculates its path and continues moving.
7. The rescue objective is completed when the drone reaches the beacon or safe path.

## Setting

The game takes place in a storm-damaged Sri Lankan jungle reserve. The visual style should feel wet, dense, and dramatic, with:

* Forest terrain
* Muddy paths
* Grass and dirt textures
* Rocks and fallen trees
* Fog or mist
* Stormy lighting
* Rescue beacons
* Autonomous rescue drones
* A heavy monster truck used for clearing paths

## Core Gameplay Features

### Player-Controlled Monster Truck

The player drives a powerful truck through the jungle to clear blocked routes. The truck should feel heavy and strong, suitable for pushing or pulling logs.

Expected truck features:

* Forward and reverse movement
* Steering controls
* Heavy vehicle feel
* Collision with fallen logs
* Optional winch mechanic
* Rigidbody-based interaction with obstacles

### Physics-Based Obstacle Clearing

Fallen trees and logs are placed as physical obstacles in the world. These obstacles block the AI drone path until the player moves them away.

Expected obstacle features:

* Box Colliders or Mesh Colliders
* Rigidbody physics
* High mass for realistic movement
* Detection when a log leaves a blocked grid cell
* Event trigger when the path becomes clear

### Autonomous Rescue Drone

The drone is an AI-controlled agent that moves automatically through the world. It does not simply follow Unity's default navigation. Instead, it uses a custom graph and AI search algorithm.

Expected drone features:

* Smooth movement between graph nodes
* Rotation toward movement direction
* Banking or tilting during turns
* Dynamic path recalculation
* Rescue beacon targeting
* Visual path debugging

### Dynamic AI Pathfinding

The AI path is not fixed. When the player clears a log, the graph changes, and the drone recalculates its route.

Example:

* A fallen tree blocks grid node `[12, 8]`.
* The node is marked as unwalkable.
* The drone avoids that route.
* The player pushes the log away.
* The node becomes walkable.
* The AI recalculates and chooses the new shorter route.

## Intelligent Systems Components

This project includes multiple intelligent systems concepts:

* Graph formulation
* Walkable and unwalkable node classification
* Adjacency list or 2D array representation
* Dynamic graph updates
* A* search algorithm
* Priority queue usage
* Heuristic function design
* BFS or UCS fallback search
* Search frontier visualization
* Real-time path recalculation

## Graphics and Visualization Components

This project also includes several graphics and visualization tasks:

* 3D terrain creation
* Lighting design
* Stormy atmosphere
* Custom drone model
* Custom rescue beacon model
* Smooth drone animation
* Debug line rendering
* Visual distinction between searched nodes and final path

## System Architecture

The project can be divided into four major systems:

```text
Player System
    ↓
Physics Interaction System
    ↓
Dynamic Graph Update System
    ↓
AI Pathfinding and Drone Movement System
```

### 1. World and Graph System

The terrain is converted into a mathematical graph. The graph represents the world using nodes and connections.

Each node stores information such as:

* Grid position
* World position
* Walkable or unwalkable state
* Cost value
* Parent node for path reconstruction
* Neighboring nodes

The graph can be stored using:

* A 2D array
* An adjacency list
* A custom `Node` class

### 2. Physics and Adaptation System

This system detects physical changes in the world. When a log moves out of a blocked area, the related node is updated.

Example graph update:

```csharp
GraphManager.UpdateNode(x, y, true);
PathfindingManager.RecalculatePath();
```

### 3. AI Search System

The AI uses A* search to find the best path from the drone position to the target.

A* uses the formula:

```text
f(n) = g(n) + h(n)
```

Where:

* `g(n)` is the cost from the start node to the current node.
* `h(n)` is the estimated cost from the current node to the goal.
* `f(n)` is the total estimated cost.

The heuristic should use Euclidean distance because the drone moves in a 3D environment and needs a realistic straight-line estimate to the target.

Euclidean distance:

```text
h(n) = sqrt((x2 - x1)^2 + (y2 - y1)^2 + (z2 - z1)^2)
```

### 4. Animation and Debug System

The drone follows the generated path smoothly using Unity interpolation methods.

Expected Unity methods:

```csharp
Vector3.Lerp()
Quaternion.Slerp()
```

The debug system uses `LineRenderer` to draw:

* Red lines for searched nodes or frontier expansion
* Green lines for the final selected path

The debug view should be toggleable, for example by pressing `Tab`.

## Team Responsibilities

## Student 1: World and Graph Architect

### Main Goal

Build the 3D jungle world and convert it into a mathematical graph for the AI system.

### Graphics Responsibilities

* Create the reserve environment using Unity Terrain.
* Sculpt the jungle landscape.
* Apply dirt, grass, and mud textures.
* Place trees, rocks, logs, and natural objects.
* Create a stormy visual mood using lighting, fog, and clouds.
* Bake the initial Unity NavMesh if needed for comparison or setup.

### Intelligent Systems Responsibilities

* Create a custom graph from the Unity world.
* Use raycasting to scan terrain.
* Detect walkable and unwalkable areas.
* Store graph nodes in a 2D array or adjacency list.
* Provide graph data to the pathfinding system.

### Suggested Implementation

1. Build the terrain first.
2. Create a grid over the terrain.
3. Cast rays downward from above each grid cell.
4. If the ray hits ground, create a walkable node.
5. If the ray hits a tree, rock, or obstacle, create an unwalkable node.
6. Connect nearby walkable nodes as neighbors.
7. Store the result as the custom graph.

## Student 2: Physics and Adaptation Engineer

### Main Goal

Make the environment interactive and update the graph when the player changes the world.

### Graphics and Physics Responsibilities

* Program monster truck movement.
* Tune vehicle controls so the truck feels heavy and powerful.
* Add colliders to fallen logs.
* Add Rigidbody components to movable logs.
* Set realistic mass and drag values.
* Allow the player to push or pull logs.

### Intelligent Systems Responsibilities

* Detect when a log is moved out of a grid cell.
* Update the graph node from unwalkable to walkable.
* Trigger AI path recalculation.
* Ensure the drone reacts to changes during gameplay.

### Suggested Implementation

1. Add a Rigidbody and collider to each fallen log.
2. Assign each log to the grid cell it blocks.
3. Track the log position during gameplay.
4. When the log moves far enough, call the graph update method.
5. Notify the pathfinding system to calculate a new route.

## Student 3: Modeler and A* Specialist

### Main Goal

Create custom 3D models and implement the primary AI brain.

### Graphics Responsibilities

* Create at least two custom 3D models using Blender or Maya.
* Required models:

  * Rescue Drone
  * Flashing Rescue Beacon
* Import the models into Unity.
* Apply suitable colors and materials.
* Ensure clean topology and usable scale.

### Intelligent Systems Responsibilities

* Implement A* search in C#.
* Create or use a priority queue.
* Calculate `g`, `h`, and `f` costs.
* Use Euclidean distance as the heuristic.
* Return the final path as a list of graph nodes.
* Explain the heuristic choice during the viva.

### Suggested A* Flow

1. Add the start node to the open list.
2. Select the node with the lowest `f` cost.
3. Move it to the closed list.
4. Check each neighbor.
5. Update cost values if a better path is found.
6. Stop when the goal node is reached.
7. Reconstruct the final path using parent nodes.

## Student 4: Animator and Debug Tech

### Main Goal

Convert pathfinding results into smooth drone movement and create visual debugging tools.

### Graphics Responsibilities

* Animate the drone along the calculated path.
* Use smooth movement instead of snapping.
* Rotate the drone naturally while flying.
* Add tilt or banking effects during turns.

### Intelligent Systems Responsibilities

* Implement a backup search algorithm such as BFS or UCS.
* Create a toggleable debug mode.
* Draw graph nodes, searched nodes, and final paths in the Unity scene.

### Suggested Implementation

1. Receive the node path from A*.
2. Move the drone from one node to the next using `Vector3.Lerp`.
3. Rotate the drone using `Quaternion.Slerp`.
4. Implement BFS as a fallback if A* fails.
5. Use `LineRenderer` to draw the debug view.
6. Toggle debug mode using the `Tab` key.

## Algorithms Used

## A* Search Algorithm

A* is the main pathfinding algorithm. It is suitable because it combines the actual cost from the start with an estimated cost to the goal.

Advantages:

* Faster than uninformed search in most cases
* Finds an optimal path when the heuristic is admissible
* Suitable for grid-based environments
* Works well with dynamic recalculation

### A* Formula

```text
f(n) = g(n) + h(n)
```

### Heuristic Function

The project uses Euclidean distance.

Reason:

The drone moves in a 3D space, and Euclidean distance gives a direct straight-line estimate between the current node and the target. This makes the heuristic suitable for flying movement, especially when the drone can move over small bumps but must avoid large obstacles.

## BFS Fallback Algorithm

Breadth-First Search can be used as a backup algorithm if A* fails.

BFS explores nodes level by level. It does not use a heuristic, so it may be slower than A*, but it is useful as a reliable fallback for unweighted graphs.

BFS is useful when:

* A* fails to return a path
* The heuristic is not suitable
* A simple guaranteed search is needed
* The graph uses equal movement costs

## Dynamic Graph Adaptation

The graph should update during gameplay when obstacles move.

Example:

```text
Before log movement:
Node [8, 4] = Unwalkable

After player clears log:
Node [8, 4] = Walkable

AI action:
Recalculate path
```

This is one of the most important intelligent system parts of the project because it shows that the AI responds to environmental changes.

## Controls

Suggested controls:

| Action              | Key   |
| ------------------- | ----- |
| Move truck forward  | W     |
| Move truck backward | S     |
| Steer left          | A     |
| Steer right         | D     |
| Brake               | Space |
| Toggle debug mode   | Tab   |
| Restart level       | R     |
| Quit game           | Esc   |

These controls can be changed depending on the final Unity implementation.

## Main Game Objects

| Game Object         | Purpose                                     |
| ------------------- | ------------------------------------------- |
| Terrain             | Main jungle reserve environment             |
| Monster Truck       | Player-controlled obstacle clearing vehicle |
| Fallen Logs         | Dynamic obstacles blocking AI paths         |
| Rescue Drone        | AI-controlled rescue agent                  |
| Rescue Beacon       | Target point for drone navigation           |
| Graph Manager       | Stores and updates graph nodes              |
| Pathfinding Manager | Runs A* and fallback search                 |
| Drone Controller    | Moves the drone along the generated path    |
| Debug Visualizer    | Displays search process and final path      |

## Suggested Unity Folder Structure

```text
Assets/
├── Animations/
├── Materials/
├── Models/
│   ├── Drone/
│   └── RescueBeacon/
├── Prefabs/
│   ├── Drone.prefab
│   ├── MonsterTruck.prefab
│   ├── FallenLog.prefab
│   └── RescueBeacon.prefab
├── Scenes/
│   └── TropicalReserveRescue.unity
├── Scripts/
│   ├── Graph/
│   │   ├── GraphManager.cs
│   │   ├── Node.cs
│   │   └── GridScanner.cs
│   ├── Pathfinding/
│   │   ├── AStarPathfinder.cs
│   │   ├── BFSPathfinder.cs
│   │   └── PriorityQueue.cs
│   ├── Player/
│   │   └── TruckController.cs
│   ├── Physics/
│   │   ├── LogObstacle.cs
│   │   └── ObstacleTrigger.cs
│   ├── AI/
│   │   └── DroneController.cs
│   └── Debug/
│       └── DebugVisualizer.cs
├── Textures/
└── UI/
```

## Technical Requirements

### Unity

Recommended Unity version:

```text
Unity 2022 LTS or newer
```

### External Tools

* Blender or Maya for custom 3D modeling
* Visual Studio or Rider for C# scripting
* Git and GitHub for version control

### Unity Components

The project may use:

* Terrain
* Rigidbody
* Colliders
* Raycast
* LineRenderer
* NavMesh
* Prefabs
* Materials
* Lighting
* Fog
* Particle effects

## Setup Instructions

1. Clone the repository.

```bash
git clone <repository-url>
```

2. Open Unity Hub.

3. Click **Add project**.

4. Select the cloned project folder.

5. Open the project using the correct Unity version.

6. Open the main scene:

```text
Assets/Scenes/TropicalReserveRescue.unity
```

7. Press **Play** in Unity.

8. Drive the monster truck and clear blocked paths.

9. Watch the drone update its path dynamically.

## Build Instructions

1. Open the project in Unity.
2. Go to **File > Build Settings**.
3. Select the target platform.
4. Add the main scene to the build list.
5. Click **Build**.
6. Choose an output folder.
7. Run the generated build.

## Testing Plan

### Graph Testing

* Check whether walkable terrain creates walkable nodes.
* Check whether trees, rocks, and logs create unwalkable nodes.
* Check whether nodes connect correctly to neighbors.
* Check whether graph data is visible in debug mode.

### Physics Testing

* Check whether the truck can push logs.
* Check whether logs respond naturally to collisions.
* Check whether logs do not fly away unrealistically.
* Check whether cleared logs update the correct graph node.

### AI Testing

* Check whether A* finds a valid path.
* Check whether the drone avoids blocked nodes.
* Check whether the path updates after obstacles move.
* Check whether BFS works if A* fails.

### Animation Testing

* Check whether drone movement is smooth.
* Check whether drone rotation follows the path.
* Check whether the drone does not snap between nodes.
* Check whether debug lines correctly show searched nodes and final path.

## Viva Preparation Notes

### Why use a graph?

The graph allows the 3D environment to be represented mathematically. Each part of the terrain becomes a node, and the AI can search through these nodes to find a valid route.

### Why use raycasting?

Raycasting allows the game to scan the terrain from above and detect whether each grid cell is ground, obstacle, tree, rock, or another object.

### Why use A*?

A* is efficient because it uses both actual movement cost and estimated distance to the goal. This helps the drone find a good path faster than basic search methods.

### Why use Euclidean distance?

Euclidean distance is suitable because the drone moves in 3D space. It estimates the direct distance from the current node to the goal.

### Why use BFS as backup?

BFS is simple and reliable for unweighted graphs. It can still find a path even if the A* implementation fails or the heuristic causes problems.

### What is dynamic adaptation?

Dynamic adaptation means the AI updates its behavior when the world changes. In this project, when the player clears a log, the graph changes and the drone recalculates its path.

### What does the debug visualizer show?

The debug visualizer shows how the AI searches the graph. It can display explored nodes, frontier nodes, and the final path selected by the algorithm.

## Expected Deliverables

* Unity 3D jungle environment
* Monster truck player controller
* Physics-based fallen log obstacles
* Custom rescue drone model
* Custom rescue beacon model
* Graph generation system
* A* pathfinding implementation
* BFS or UCS fallback search
* Dynamic graph update system
* Smooth drone movement
* Toggleable debug visualization
* Final playable build
* Project report or viva explanation

## Possible Future Improvements

* Add multiple drones
* Add multiple stranded hikers or animals
* Add fuel or time limits
* Add weather effects such as rain and lightning
* Add minimap or mission UI
* Add winch controls for the truck
* Add sound effects for storm, truck, drone, and rescue beacon
* Add difficulty levels
* Add procedural obstacle placement
* Add performance comparison between A*, BFS, and UCS

## Project Status

Current stage:

```text
Game proposal and system design completed.
```

Next recommended stage:

```text
Build the Unity terrain, create the graph scanner, and implement basic A* pathfinding.
```

## License

This project is created for academic purposes. Update this section if the repository needs a specific open-source license.

## Credits

Project title: **Tropical Reserve Rescue**

Concept: A Sri Lankan storm-damaged wildlife reserve rescue simulation using Unity, physics interaction, graph formulation, and AI pathfinding.

Developed as a group assignment project.
