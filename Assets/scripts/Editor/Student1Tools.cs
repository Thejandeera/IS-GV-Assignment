using UnityEngine;
using UnityEditor;

public class Student1Tools : EditorWindow
{
    [MenuItem("Student 1 Tools/Setup Obstacles (Trees & Rocks)")]
    public static void SetupObstacles()
    {
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        if (obstacleLayer == -1)
        {
            Debug.LogError("Please create a Layer called 'Obstacle' in Unity first! (Edit -> Project Settings -> Tags and Layers)");
            return;
        }

        int count = 0;

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

                count++;
            }
        }

        Debug.Log($"<color=green><b>Success!</b></color> Assigned the Obstacle layer and Colliders to {count} trees/rocks/logs in the scene.");
    }

    [MenuItem("Student 1 Tools/Make Atmosphere Stormy")]
    public static void MakeStormy()
    {
        Undo.RecordObject(RenderSettings.sun, "Change Lighting");

        Material stormSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.mat");
        if (stormSky != null)
        {
            RenderSettings.skybox = stormSky;
        }

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.2f, 0.22f, 0.25f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.012f;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.55f;

        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional)
            {
                Undo.RecordObject(l, "Change Light");
                l.color = new Color(0.3f, 0.35f, 0.45f);
                l.intensity = 0.6f;
                l.shadowStrength = 0.8f;
                l.shadows = LightShadows.Soft;
            }
        }

        DynamicGI.UpdateEnvironment();

        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.name == "LightningSystem" && !EditorUtility.IsPersistent(obj.transform.root.gameObject))
            {
                obj.SetActive(true);
            }
        }

        Debug.Log("<color=cyan><b>Storm Atmosphere Applied!</b></color> Dark and moody lighting updated.");
    }

    [MenuItem("Student 1 Tools/Reset Atmosphere to Normal")]
    public static void MakeNormal()
    {
        Material sunnySky = AssetDatabase.LoadAssetAtPath<Material>("Assets/NatureStarterKit2/Materials/skybox.mat");

        if (sunnySky != null)
        {
            RenderSettings.skybox = sunnySky;
        }

        RenderSettings.fog = false;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f);

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

        DynamicGI.UpdateEnvironment();

        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.name == "LightningSystem" && !EditorUtility.IsPersistent(obj.transform.root.gameObject))
            {
                obj.SetActive(false);
            }
        }

        Debug.Log("<color=yellow><b>Atmosphere Reset!</b></color> Skybox returned to normal.");
    }
}