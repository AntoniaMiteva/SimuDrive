using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class RoadJunctionSecondRoad : MonoBehaviour
{
	[SerializeField] private CarController carController;
	[SerializeField] private TextMeshProUGUI textMeshProUGUI;
	[SerializeField] private GameObject panelProblem;
	[SerializeField] private GameObject panelProblemCollision;
	[SerializeField] private GameObject panelDone;
	[SerializeField] private GameObject panelInstruction;
	[SerializeField] private GameObject panelDidntStop;
	[SerializeField] private GameObject panelHitCar;
	[SerializeField] private Button backButton;
	[SerializeField] private Button restartButton;
	[SerializeField] private Button restartButtonDidntStop;
	[SerializeField] private Button restartButtonHitCar;

	private float clutchInput;
	private float acceleratorInput;
	private float previousClutchInput;
	private bool levelCompleted = false;

	public static bool carOutsideTheRoad = false;

	public static bool isCarStart = false;

	private enum StepState { Step1, Step2, Step3, Completed }
	private StepState currentStep = StepState.Step1;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		panelProblem.SetActive(false);
		panelDone.SetActive(false);
		panelInstruction.SetActive(true);
		panelProblemCollision.SetActive(false);

		carOutsideTheRoad = false;
		isCarStart = false;


		textMeshProUGUI.text = "Потеглете.";

		SetupEventSystem();
		SetupBackButton();
		SetupRestartButton();
		SetupRestartButtonDidntStop();
		SetupRestartButtonHitCar();
	}

	// Update is called once per frame
	void Update()
	{
		if (panelProblemCollision.activeSelf)
		{
			Time.timeScale = 1f;
			return;
		}

		if (!levelCompleted)
		{
			clutchInput = Input.GetAxis("Clutch");
			acceleratorInput = Input.GetAxis("Accelerator");
			previousClutchInput = clutchInput;

			if (panelDone.activeSelf == false)
			{
				if (carController.Speed <= 5f && carController.Gear == 1 && !(clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift)))
				{
					panelProblem.SetActive(true);
					Time.timeScale = 0f;
				}
				else
				{
					panelProblem.SetActive(false);
					Time.timeScale = 1f;
				}
			}

			ProcessSteps();
		}

		if (carController.carObstacle)
		{
			panelProblemCollision.SetActive(true);
			panelProblem.SetActive(false);
			carController.speed = 0f;
			carController.enabled = false;
			Time.timeScale = 1f;
			carOutsideTheRoad = true;
		}
		else
		{
			panelProblemCollision.SetActive(false);
			Time.timeScale = 1f;
		}

		if (carController.carCubeFront)
		{
			panelDidntStop.SetActive(true);
			panelProblemCollision.SetActive(false);
			panelProblem.SetActive(false);
			panelDone.SetActive(false);
			textMeshProUGUI.text = "";
			carController.speed = 0f;
			carController.enabled = false;
			Time.timeScale = 1f;
		}
		else
		{
			panelDidntStop.SetActive(false);
			Time.timeScale = 1f;
		}

		if (carController.carAnotherCar)
		{
			panelHitCar.SetActive(true);
			panelProblemCollision.SetActive(false);
			panelProblem.SetActive(false);
			carController.speed = 0f;
			carController.enabled = false;
			Time.timeScale = 1f;
		}
		else
		{
			panelHitCar.SetActive(false);
			Time.timeScale = 1f;
		}
	}

	private void ProcessSteps()
	{
		switch (currentStep)
		{
			case StepState.Step1:
				if (carController.Speed > 3)
				{
					isCarStart = true;
					textMeshProUGUI.text = "Преминете безопасно през кръстовището без да отнемате предимство. При необходимост, спрете.";
					currentStep = StepState.Step2;
				}
				break;
			case StepState.Step2:
				if (carController.carFinishLevel == true)
				{
					currentStep = StepState.Completed;
				}
				break;
			case StepState.Completed:
				CompleteLevel();
				break;
		}
	}

	private void CompleteLevel()
	{
		Debug.Log("Level completed - showing completion panel");
		levelCompleted = true;
		textMeshProUGUI.text = "Успешно мнахте през кръстовището.";
		carController.enabled = false;
		Time.timeScale = 1f;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		StartCoroutine(ShowCompletionPanel());
	}

	private System.Collections.IEnumerator ShowCompletionPanel()
	{
		yield return new WaitForSeconds(0.5f);
		panelDone.SetActive(true);
		panelProblem.SetActive(false);
		textMeshProUGUI.text = "";

		Debug.Log("Completion panel activated");

		if (backButton != null)
		{
			backButton.interactable = true;
		}
	}

	public void BackToStart()
	{
		Debug.Log("BackToStart called from UI event");
		ManualBackToStart();
	}

	public void ManualBackToStart()
	{
		Debug.Log("Returning to menu");
		Time.timeScale = 1f;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		SceneManager.LoadScene("Drive Menu");
	}
	void SetupRestartButton()
	{
		if (restartButton != null)
		{
			restartButton.onClick.RemoveAllListeners();
			restartButton.onClick.AddListener(RestartLevel);
		}
	}

	void SetupRestartButtonDidntStop()
	{
		if (restartButton != null)
		{
			restartButtonDidntStop.onClick.RemoveAllListeners();
			restartButtonDidntStop.onClick.AddListener(RestartLevel);
		}
	}

	void SetupRestartButtonHitCar()
	{
		if (restartButtonHitCar != null)
		{
			restartButtonHitCar.onClick.RemoveAllListeners();
			restartButtonHitCar.onClick.AddListener(RestartLevel);
		}
	}


	public void RestartLevel()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void StartAgain()
	{
		SceneManager.LoadScene("try2");
	}

	void SetupEventSystem()
	{
		if (FindObjectOfType<EventSystem>() == null)
		{
			GameObject eventSystem = new GameObject("EventSystem");
			eventSystem.AddComponent<EventSystem>();
			eventSystem.AddComponent<StandaloneInputModule>();
		}
	}

	void SetupBackButton()
	{
		if (backButton == null)
		{
			Button[] buttons = panelDone.GetComponentsInChildren<Button>(true);
			if (buttons.Length > 0) backButton = buttons[0];
		}

		if (backButton != null)
		{
			backButton.onClick.RemoveAllListeners();
			backButton.onClick.AddListener(ManualBackToStart);
		}
	}
}
