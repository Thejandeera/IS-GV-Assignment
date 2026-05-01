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

        // 1. Fog Setup
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.2f, 0.25f, 0.3f); // Dark blue/grey
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.015f;

        // 2. Ambient Lighting Setup (Dark and moody)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.15f, 0.18f, 0.25f);

        // 3. Directional Light Setup
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional)
            {
                Undo.RecordObject(l, "Change Light");
                l.color = new Color(0.4f, 0.45f, 0.6f); // Cold storm light
                l.intensity = 0.6f;
                l.shadowStrength = 0.8f;

                // Turn on soft shadows if they aren't already
                l.shadows = LightShadows.Soft;
            }
        }

        Debug.Log("<color=cyan><b>Storm Atmosphere Applied!</b></color> Look at your Game View to see the moody lighting and fog.");
    }

    [MenuItem("Student 1 Tools/Reset Atmosphere to Normal")]
    public static void MakeNormal()
    {
        // 1. Fog Setup
        RenderSettings.fog = false;

        // 2. Ambient Lighting Setup (Fixed for black shadows)
        // Using "Flat" instead of "Skybox" guarantees the shadows will be filled with light immediately
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f); // Soft, bright sky-blue bounce light

        // 3. Directional Light Setup
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional)
            {
                Undo.RecordObject(l, "Reset Light");
                l.color = new Color(1f, 0.95f, 0.83f); // Warm sun light
                l.intensity = 1.2f; // Slightly boost the sun intensity for URP

                // 4. FIX THE SHARPNESS AND DARKNESS HERE
                l.shadows = LightShadows.Soft; // Force shadows to have soft, realistic edges
                l.shadowStrength = 0.7f;       // Make shadows slightly transparent so we can see the grass inside them
            }
        }

        Debug.Log("<color=yellow><b>Atmosphere Reset!</b></color> Your lighting is back to the default realistic view.");
    }
}
