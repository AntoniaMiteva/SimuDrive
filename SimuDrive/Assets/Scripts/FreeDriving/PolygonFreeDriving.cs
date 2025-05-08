using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PolygonFreeDriving : MonoBehaviour
{
	[SerializeField] private CarController carController;
	[SerializeField] private GameObject panelProblem;
	[SerializeField] private GameObject panelProblemCollision;
	[SerializeField] private Button quitButton;
	[SerializeField] private GameObject panelQuit;
	[SerializeField] private Button restartButton;
	[SerializeField] private Button buttonAnotherCar;
	[SerializeField] private GameObject panelAnotherCar;

	void Start()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		panelProblem.SetActive(false);
		panelQuit.SetActive(false);
		panelAnotherCar.SetActive(false);

		SetupRestartButton();
		SetupRestartButtonAnotherCar();


		// Check if EventSystem exists
		if (FindObjectOfType<EventSystem>() == null)
		{
			GameObject eventSystem = new GameObject("EventSystem");
			eventSystem.AddComponent<EventSystem>();
			eventSystem.AddComponent<StandaloneInputModule>();
		}

	}
	void Update()
    {
		if (carController.carObstacle)
		{
			panelProblemCollision.SetActive(true);
			panelProblem.SetActive(false);
			carController.speed = 0f;
			carController.enabled = false;
			Time.timeScale = 1f;
		}
		else
		{
			panelProblemCollision.SetActive(false);
			Time.timeScale = 1f;
		}

		if (carController.carAnotherCar)
		{
			panelAnotherCar.SetActive(true);
			panelProblem.SetActive(false);
			carController.speed = 0f;
			carController.enabled = false;
			Time.timeScale = 1f;
		}
		else
		{
			panelAnotherCar.SetActive(false);
			Time.timeScale = 1f;
		}


		if (Input.GetKey(KeyCode.Escape))
		{
			panelQuit.SetActive(true);
			panelProblem.SetActive(false);
		}
	}

	void SetupRestartButton()
	{
		if (restartButton != null)
		{
			restartButton.onClick.RemoveAllListeners();
			restartButton.onClick.AddListener(RestartLevel);
		}
	}

	void SetupRestartButtonAnotherCar()
	{
		if (restartButton != null)
		{
			buttonAnotherCar.onClick.RemoveAllListeners();
			buttonAnotherCar.onClick.AddListener(RestartLevel);
		}
	}
	public void RestartLevel()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
