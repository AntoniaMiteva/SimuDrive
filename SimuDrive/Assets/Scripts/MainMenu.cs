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

    public void OptionsButton()
    {
        SceneManager.LoadScene("Options Menu");
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
