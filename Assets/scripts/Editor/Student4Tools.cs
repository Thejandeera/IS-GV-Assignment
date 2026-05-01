using UnityEngine;
using UnityEditor;

public class Student4Tools : EditorWindow
{
    [MenuItem("Student 4 Tools/Use A* Search (Primary)")]
    public static void SwitchToAStar()
    {
        DroneMovement drone = Object.FindObjectOfType<DroneMovement>();
        if (drone != null)
        {
            Undo.RecordObject(drone, "Change Algorithm to A*");
            drone.currentAlgorithm = DroneMovement.ActiveAlgorithm.AStar;
            Debug.Log("<color=green><b>Success!</b></color> Drone algorithm set to Primary A* Search.");
        }
        else
        {
            Debug.LogError("Could not find a DroneMovement script in the scene!");
        }
    }

    [MenuItem("Student 4 Tools/Use BFS Search (Backup)")]
    public static void SwitchToBFS()
    {
        DroneMovement drone = Object.FindObjectOfType<DroneMovement>();
        if (drone != null)
        {
            Undo.RecordObject(drone, "Change Algorithm to BFS");
            drone.currentAlgorithm = DroneMovement.ActiveAlgorithm.BFS;
            Debug.Log("<color=green><b>Success!</b></color> Drone algorithm set to Secondary BFS Search.");
        }
        else
        {
            Debug.LogError("Could not find a DroneMovement script in the scene!");
        }
    }
}
