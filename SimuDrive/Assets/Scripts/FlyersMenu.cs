using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlyersMenu : MonoBehaviour
{
    public void BackButton()
    {
        SceneManager.LoadScene("Main Menu");
    }
    
    public void OpenURL()
    {
        Application.OpenURL("https://avtoizpit.com/");
    }
}