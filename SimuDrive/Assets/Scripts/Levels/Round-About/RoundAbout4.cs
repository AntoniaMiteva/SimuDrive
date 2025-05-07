using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class RoundAbout4 : MonoBehaviour
{
	[SerializeField] private CarController carController;
	[SerializeField] private TextMeshProUGUI textMeshProUGUI;
	[SerializeField] private GameObject panelProblem;
	[SerializeField] private GameObject panelProblemCollision;
	[SerializeField] private GameObject panelProblemWrongBlinker;
	[SerializeField] private GameObject panelProblemNoBlinker;
	[SerializeField] private GameObject panelDone;
	[SerializeField] private GameObject panelInstruction;
	[SerializeField] private Button backButton;
	[SerializeField] private Button restartButton;
	[SerializeField] private Button restartButtonNoBlinker;
	[SerializeField] private Button restartButtonWrongBlinker;

	private float clutchInput;
	private float acceleratorInput;
	private float previousClutchInput;
	private bool levelCompleted = false;
	private bool cubeBack = false;
	private bool cubeLeft = false;
	private bool cubeRight = false;

	public static bool carOutsideTheRoad = false;
	private bool wrongBlinker = false;
	private bool isBlinker = false;

	private enum StepState { Step1, Step2, Step3, Step4, Completed }
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
		panelProblemWrongBlinker.SetActive(false);

		carOutsideTheRoad = false;

		textMeshProUGUI.text = "Уверете се, че във вашата лента не идва кола от кръговото и потеглете.";

		SetupEventSystem();
		SetupBackButton();
		SetupRestartButton();
		SetupRestartButtonNoBlinker();
		SetupRestartButtonWrongBlinker();
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

		if (wrongBlinker)
		{
			panelProblemWrongBlinker.SetActive(true);
			panelProblemCollision.SetActive(false);
			panelProblem.SetActive(false);
			carController.speed = 0f;
			carController.enabled = false;
			Time.timeScale = 1f;
		}
		else
		{
			panelProblemWrongBlinker.SetActive(false);
			Time.timeScale = 1f;
		}

		if (carController.carBlinkChecker && !isBlinker)
		{
			panelProblemNoBlinker.SetActive(true);
			panelProblemCollision.SetActive(false);
			panelProblem.SetActive(false);
			carController.speed = 0f;
			carController.enabled = false;
			Time.timeScale = 1f;
		}
		else
		{
			panelProblemNoBlinker.SetActive(false);
			Time.timeScale = 1f;
		}

		if (carController.carCubeBack)
		{
			cubeBack = true;
		}

		if (carController.carCubeLeft)
		{
			cubeLeft = true;
		}

		if (carController.carCubeRight)
		{
			cubeRight = true;
		}
	}

	private void ProcessSteps()
	{
		switch (currentStep)
		{
			case StepState.Step1:
				if (carController.Speed > 5)
				{
					textMeshProUGUI.text = "Пуснете левия си мигач.";
					currentStep = StepState.Step2;
				}
				else
				{
					textMeshProUGUI.text = "Уверете се, че във вашата лента не идва кола от кръговото и потеглете.";
				}
				break;

			case StepState.Step2:
				if (carController.isLeftBlinker && carController.carBlinkChecker2)
				{
					textMeshProUGUI.text = "Пуснете десния си мигач.";
					isBlinker = true;
					carController.isLeftBlinker= false;
					currentStep = StepState.Step3;
				}
				else if (carController.isRightBlinker)
				{
					wrongBlinker = true;
				}
				break;
			case StepState.Step3:
				if (carController.isRightBlinker)
				{
					textMeshProUGUI.text = "Излесте от кръговото, като спазвате лентата си.";
					isBlinker = true;
					carController.isRightBlinker = false;
					currentStep = StepState.Step4;
				}
				else if (carController.isLeftBlinker)
				{
					wrongBlinker = true;
				}
				break;
			case StepState.Step4:
				if (carController.carEndOfTheRoad == true)
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

		// Mark level as completed
		levelCompleted = true;

		// Update text and disable car controller
		textMeshProUGUI.text = "Успешно мнахте през кръговото.";
		carController.enabled = false;

		// Important: Keep time running for UI interactions
		Time.timeScale = 1f;

		// Set cursor state
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		// Hide text after showing panel
		StartCoroutine(ShowCompletionPanel());
	}

	private System.Collections.IEnumerator ShowCompletionPanel()
	{
		// Small delay to ensure text is seen
		yield return new WaitForSeconds(0.5f);

		// Show completion panel
		panelDone.SetActive(true);
		panelProblem.SetActive(false);
		textMeshProUGUI.text = "";

		Debug.Log("Completion panel activated");

		// Make sure button is interactive
		if (backButton != null)
		{
			backButton.interactable = true;
		}
	}

	// Method called directly by button
	public void BackToStart()
	{
		Debug.Log("BackToStart called from UI event");
		ManualBackToStart();
	}

	// Alternative method called programmatically
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

	void SetupRestartButtonWrongBlinker()
	{
		if (restartButton != null)
		{
			restartButtonWrongBlinker.onClick.RemoveAllListeners();
			restartButtonWrongBlinker.onClick.AddListener(RestartLevel);
		}
	}

	void SetupRestartButtonNoBlinker()
	{
		if (restartButton != null)
		{
			restartButtonNoBlinker.onClick.RemoveAllListeners();
			restartButtonNoBlinker.onClick.AddListener(RestartLevel);
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
