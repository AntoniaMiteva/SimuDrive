using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlyersMenu : MonoBehaviour
{
    private bool isPaused = false;

    public void BackButton()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void OpenURL()
    {
        StartCoroutine(OpenURLAsync("https://avtoizpit.com/"));
    }

    private IEnumerator OpenURLAsync(string url)
    {
        yield return null; // Wait for a frame to ensure the game continues running
        Application.OpenURL(url);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
        if (isPaused)
        {
            // Handle pause (e.g., stop audio, animations, etc.)
            Time.timeScale = 0; // Pause the game
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!isPaused && hasFocus)
        {
            // Resume the game when focus is regained
            Time.timeScale = 1; // Unpause the game
        }
    }
}