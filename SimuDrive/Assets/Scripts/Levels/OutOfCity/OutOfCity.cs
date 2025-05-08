using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class OutOfCity : MonoBehaviour
{
	[SerializeField] private GameObject panelQuit;
	[SerializeField] private Button quitButton;

	void Update()
    {
		if (Input.GetKey(KeyCode.Escape))
		{
			panelQuit.SetActive(true);
		}
	}
}
