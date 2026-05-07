using UnityEngine;
using UnityEditor;

public class Student2Tools : EditorWindow
{
    // Menu item to stop all trees from falling
    [MenuItem("Student 2 Tools/System Engineer/Deactivate Tree Fall")]
    public static void StopTreeFalls()
    {
        TreeFall[] trees = Object.FindObjectsOfType<TreeFall>();
        foreach (TreeFall tree in trees)
        {
            Undo.RecordObject(tree, "Deactivate Tree Fall");
            tree.enabled = false;
        }
        Debug.Log("<color=red><b>[System]</b></color> All tree fall scripts disabled.");
    }

    // Menu item to activate all trees for falling
    [MenuItem("Student 2 Tools/System Engineer/Activate Tree Fall")]
    public static void ResumeTreeFalls()
    {
        TreeFall[] trees = Object.FindObjectsOfType<TreeFall>();
        foreach (TreeFall tree in trees)
        {
            Undo.RecordObject(tree, "Activate Tree Fall");
            tree.enabled = true;
        }
        Debug.Log("<color=green><b>[System]</b></color> All tree fall scripts enabled.");
    }

    // Menu item to increase the falling speed/force of all trees at once
    [MenuItem("Student 2 Tools/System Engineer/Set High Impact Force")]
    public static void SetHighForce()
    {
        TreeFall[] trees = Object.FindObjectsOfType<TreeFall>();
        foreach (TreeFall tree in trees)
        {
            Undo.RecordObject(tree, "Set High Force");
            tree.fallForce = 100f; // Increase this value as needed
        }
        Debug.Log("<color=cyan><b>[System]</b></color> All trees set to High Impact (Force: 100).");
    }
}