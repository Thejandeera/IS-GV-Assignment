using UnityEngine;
using UnityEditor;

public class Student1Tools : EditorWindow
{
    [MenuItem("Student 1 Tools/Setup Obstacles (Trees & Rocks)")]
    public static void SetupObstacles()
    {
        // Find or Create the Obstacle layer
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        if (obstacleLayer == -1)
        {
            Debug.LogError("Please create a Layer called 'Obstacle' in Unity first! (Edit -> Project Settings -> Tags and Layers)");
            return;
        }

        int count = 0;

        // Find all MeshRenderers in the scene (since trees and rocks usually have meshes)
        MeshRenderer[] allRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);

        foreach (MeshRenderer r in allRenderers)
        {
            GameObject go = r.gameObject;
            string name = go.name.ToLower();

            // If the name contains tree, rock, or log, we assume it's an obstacle
            if (name.Contains("tree") || name.Contains("rock") || name.Contains("log") || name.Contains("stump") || name.Contains("bush"))
            {
                Undo.RecordObject(go, "Setup Obstacle");

                // 1. Set the Layer to Obstacle
                go.layer = obstacleLayer;

                // 2. Add a collider if it doesn't have one
                Collider col = go.GetComponent<Collider>();
                if (col == null)
                {
                    // Prefer MeshCollider for rocks, Capsule/Box for trees
                    if (name.Contains("tree"))
                    {
                        CapsuleCollider cap = go.AddComponent<CapsuleCollider>();
                        // Give it a reasonable default size for a tree trunk
                        cap.radius = 0.5f;
                        cap.height = 5f;
                    }
                    else
                    {
                        go.AddComponent<MeshCollider>();
                    }
                }

                count++;
            }
        }

        Debug.Log($"<color=green><b>Success!</b></color> Assigned the Obstacle layer and Colliders to {count} trees/rocks/logs in the scene.");
    }

    [MenuItem("Student 1 Tools/Make Atmosphere Stormy")]
    public static void MakeStormy()
    {
        Undo.RecordObject(RenderSettings.sun, "Change Lighting");

        // --- Swap to the exact Overcast material ---
        Material stormSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.mat");
        if (stormSky != null)
        {
            RenderSettings.skybox = stormSky;
        }

        // 1. Fog Setup (FIXED: Much darker fog color to remove the "whiteness")
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.2f, 0.22f, 0.25f); // Dark charcoal/navy blue
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.012f; // Slightly lowered so you can still see the map

        // 2. Ambient Lighting Setup (FIXED: Lower intensity so shadows are actually dark)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.55f; // Barely bouncing any light

        // 3. Directional Light Setup (FIXED: Dimmer, colder sun)
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional)
            {
                Undo.RecordObject(l, "Change Light");
                l.color = new Color(0.3f, 0.35f, 0.45f); // Deep cold blue
                l.intensity = 0.6f; // Sun is heavily blocked by clouds
                l.shadowStrength = 0.8f; // Stronger shadows
                l.shadows = LightShadows.Soft;
            }
        }

        // Tell Unity to recalculate the lighting based on the new sky
        DynamicGI.UpdateEnvironment();

        // Turn ON the Lightning System
        GameObject lightning = GameObject.Find("LightningSystem");
        if (lightning != null) lightning.SetActive(true);

        Debug.Log("<color=cyan><b>Storm Atmosphere Applied!</b></color> Dark and moody lighting updated.");
    }

    [MenuItem("Student 1 Tools/Reset Atmosphere to Normal")]
    public static void MakeNormal()
    {
        // --- Swap back to the Nature Starter Kit sky ---
        Material sunnySky = AssetDatabase.LoadAssetAtPath<Material>("Assets/NatureStarterKit2/Materials/skybox.mat");

        if (sunnySky != null)
        {
            RenderSettings.skybox = sunnySky;
        }

        // 1. Fog Setup
        RenderSettings.fog = false;

        // 2. Ambient Lighting Setup (FIXED: Changed to Flat so it stays bright in Play Mode!)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f); // Soft, bright sky-blue bounce light

        // 3. Directional Light Setup
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional)
            {
                Undo.RecordObject(l, "Reset Light");
                l.color = new Color(1f, 0.95f, 0.83f);
                l.intensity = 1.2f;
                l.shadows = LightShadows.Soft;
                l.shadowStrength = 0.7f;
            }
        }

        // Tell Unity to recalculate the lighting
        DynamicGI.UpdateEnvironment();

        // Turn OFF the Lightning System
        GameObject lightning = GameObject.Find("LightningSystem");
        if (lightning != null) lightning.SetActive(false);

        Debug.Log("<color=yellow><b>Atmosphere Reset!</b></color> Skybox returned to normal.");
    }
}