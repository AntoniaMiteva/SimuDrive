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

    public void ChamgeGears1()
    {
        SceneManager.LoadScene("ChangeGears-1");
    }
}
