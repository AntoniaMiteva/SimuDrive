using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DriveMenu : MonoBehaviour
{
    public void BackButton()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void StartUpButton()
    {
        SceneManager.LoadScene("StartDriveScene");
    }

    public void StartUp1()
    {
        SceneManager.LoadScene("Start-Up-1");
    }
    public void StartUp2()
    {
        SceneManager.LoadScene("Start-Up-2");
    }
    public void StartUp3()
    {
        SceneManager.LoadScene("Start-Up-3");
    }

	public void ChangeGears1()
	{
		SceneManager.LoadScene("ChangeGears-1");
	}

	public void ChangeGears2()
	{
		SceneManager.LoadScene("ChangeGears-2");
	}

	public void Overtake1()
	{
		SceneManager.LoadScene("Overtake-1");
	}

	public void RoundAbout1()
	{
		SceneManager.LoadScene("Round-About-1");
	}
}
