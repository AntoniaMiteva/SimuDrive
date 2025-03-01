using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Load Scenes
    public void PlayButton()
    {
        SceneManager.LoadScene("Drive Menu");
    }

    public void InstructionsButton()
    {
        SceneManager.LoadScene("Instructions Menu");
    }

    public void SettingsButton()
    {
        SceneManager.LoadScene("Settings Menu");
    }

    public void FlyersButton()
    {
        SceneManager.LoadScene("Flyers Menu");
    }
    //Quit Game
    public void QuitButton()
    {
        Application.Quit();
        Debug.Log("The Player has Quit the game");
    }
}
