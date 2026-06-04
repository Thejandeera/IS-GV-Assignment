# Student1Tools.cs - Explanation

## Role Alignment
**Graphics and Visualization (GV): The World Builder**
This script is an Editor tool designed to automate the process of building the 3D world, specifically focusing on setting up collision for level design, and baking/modifying the lighting and atmospheric effects dynamically.

## Code Explanation & Mechanism

### 1. `SetupObstacles()` Method
This method is used to automate the assignment of layers and colliders to environmental assets like trees, rocks, and bushes. 

```csharp
[MenuItem("Student 1 Tools/Setup Obstacles (Trees & Rocks)")]
public static void SetupObstacles()
{
    int obstacleLayer = LayerMask.NameToLayer("Obstacle");
    // ... validation checks ...

    MeshRenderer[] allRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

    foreach (MeshRenderer r in allRenderers)
    {
        GameObject go = r.gameObject;
        string name = go.name.ToLower();

        if (name.Contains("tree") || name.Contains("rock") || name.Contains("log") || name.Contains("stump") || name.Contains("bush"))
        {
            Undo.RecordObject(go, "Setup Obstacle");
            go.layer = obstacleLayer;

            Collider col = go.GetComponent<Collider>();
            if (col == null)
            {
                if (name.Contains("tree"))
                {
                    CapsuleCollider cap = go.AddComponent<CapsuleCollider>();
                    cap.radius = 0.5f;
                    cap.height = 5f;
                }
                else
                {
                    go.AddComponent<MeshCollider>();
                }
            }
        }
    }
}
```
**Mechanism & Theory:**
- **Automation in Level Design:** Instead of manually adding colliders and setting layers for thousands of trees and rocks, this script iterates through all `MeshRenderer` components in the scene.
- **Layer Assignment:** It assigns the objects to the "Obstacle" layer. This is crucial for the Intelligent Systems (IS) component, as the `GridManager` uses a Raycast against this specific layer to determine if a grid node is walkable or blocked.
- **Collider Generation:** It dynamically adds `CapsuleColliders` to trees (for optimized physics calculations) and `MeshColliders` to rocks/irregular shapes for accurate raycasting.

### 2. `MakeStormy()` and `MakeNormal()` Methods
These methods control the visual atmosphere, lighting, and textures of the environment.

```csharp
[MenuItem("Student 1 Tools/Make Atmosphere Stormy")]
public static void MakeStormy()
{
    // Skybox and Fog
    Material stormSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.mat");
    RenderSettings.skybox = stormSky;
    RenderSettings.fog = true;
    RenderSettings.fogColor = new Color(0.2f, 0.22f, 0.25f);
    RenderSettings.fogDensity = 0.012f;

    // Ambient Lighting
    RenderSettings.ambientIntensity = 0.55f;

    // Directional Light
    Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
    foreach (Light l in allLights)
    {
        if (l.type == LightType.Directional)
        {
            l.color = new Color(0.3f, 0.35f, 0.45f);
            l.intensity = 0.6f;
        }
    }
    DynamicGI.UpdateEnvironment();
}
```
**Mechanism & Theory:**
- **Lighting and Texturing:** Modifies the global `RenderSettings`. It changes the skybox material, enables exponential fog to create a dense, moody atmosphere, and darkens the ambient color.
- **Directional Light Adjustments:** It finds the main sun (Directional Light) and reduces its intensity and color to match an overcast, stormy look.
- **Global Illumination (GI):** Calls `DynamicGI.UpdateEnvironment()` to force Unity to recalculate the ambient lighting bounces based on the new skybox.
- **System Integration:** It activates the `LightningSystem` (handled by `LightningController.cs`), showing how the environment design integrates with dynamic weather elements.
