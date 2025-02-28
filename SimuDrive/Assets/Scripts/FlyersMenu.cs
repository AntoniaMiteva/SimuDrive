using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlyerMenu : MonoBehaviour
{
    public GameObject panel;
    public void OpenURL()
    {
        Application.OpenURL("https://avtoizpit.com/");
    }
}