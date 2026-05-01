using UnityEngine;
using System.Collections;

// We need this to make the "Quit" button work inside the Unity Editor
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CaveEntrance : MonoBehaviour
{
    private bool isPlayerNear = false;
    private bool isGameOver = false;

    // This detects when the player walks INTO the Box Collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "character")
        {
            isPlayerNear = true;
        }
    }

    // This detects if the player walks AWAY from the cave
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "character")
        {
            isPlayerNear = false;
        }
    }

    void Update()
    {
        // If the player is standing in the trigger and presses Enter...
        if (isPlayerNear && !isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StartCoroutine(GameOverSequence());
            }
        }
    }

    IEnumerator GameOverSequence()
    {
        isGameOver = true;

        // Freeze time so the player and drone stop moving
        Time.timeScale = 0f;

        // Wait for 3 seconds so the player can read "GAME OVER"
        yield return new WaitForSecondsRealtime(3f);

        Debug.Log("Exiting Game...");

        // Quit the game (Works in both the Editor and the final built game)
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- PROFESSIONAL UI DISPLAY ---
    void OnGUI()
    {
        if (isGameOver)
        {
            // Draw a full-screen black overlay
            GUI.color = new Color(0, 0, 0, 0.9f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            // Draw giant red GAME OVER text in the center
            GUI.color = Color.red;
            GUIStyle gameOverStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 80,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            gameOverStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "GAME OVER", gameOverStyle);
        }
        else if (isPlayerNear)
        {
            // Draw a sleek dark box at the bottom of the screen
            GUI.color = new Color(0, 0, 0, 0.85f);
            Rect boxRect = new Rect(Screen.width / 2 - 200, Screen.height - 150, 400, 60);
            GUI.Box(boxRect, "");

            // Draw the prompt text
            GUI.color = Color.white;
            GUIStyle promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            promptStyle.normal.textColor = Color.white;
            GUI.Label(boxRect, "Press [ENTER] to enter the cave", promptStyle);
        }
    }
}