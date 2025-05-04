using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Overtake : MonoBehaviour
{
	[SerializeField] private CarController carController;
	[SerializeField] private TextMeshProUGUI textMeshProUGUI;
	[SerializeField] private GameObject panelProblem;
	[SerializeField] private GameObject panelProblemCollision;
	[SerializeField] private GameObject panelProblemAnotherCar;
	[SerializeField] private GameObject panelDone;
	[SerializeField] private GameObject panelInstruction;
	[SerializeField] private Button backButton;
	[SerializeField] private Button restartButton;
	[SerializeField] private Button restartButtonAnotherCarPanel;

	private float clutchInput;
	private float acceleratorInput;
	private float previousClutchInput;
	private bool levelCompleted = false;
	private bool cubeBack = false;
	private bool cubeLeft = false;
	private bool cubeRight = false;

	public static bool carOutsideTheRoad = false;

	private enum StepState { Step1, Step2, Step3, Step4, Step5, Step6, Completed }
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
		panelProblemAnotherCar.SetActive(false);

		carOutsideTheRoad = false;

		textMeshProUGUI.text = "Запалете колата и потеглете.";

		SetupEventSystem();
		SetupBackButton();
		SetupRestartButton();
		SetupRestartButtonAnotherCar();

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
			carController.enabled = false;
			Time.timeScale = 1f;
			carOutsideTheRoad = true;
		}
		else
		{
			panelProblemCollision.SetActive(false);
			Time.timeScale = 1f;
		}

		if (carController.carAnotherCar)
		{
			panelProblemAnotherCar.SetActive(true);
			panelProblem.SetActive(false);
			carController.enabled = false;
			Time.timeScale = 1f;
			carOutsideTheRoad = true;
		}
		else
		{
			panelProblemAnotherCar.SetActive(false);
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
				if (carController.Speed>5)
				{
					textMeshProUGUI.text = "Настигнете автомобила пред вас.";
					currentStep = StepState.Step2;
				}
				else
				{
					textMeshProUGUI.text = "Запалете колата и потеглете.";
				}
				break;

			case StepState.Step2:
				if (cubeBack)
				{
					textMeshProUGUI.text = "Пуснете левия си мигач.";
					currentStep = StepState.Step3;
				}
				break;
			case StepState.Step3:
				if (cubeBack && carController.isLeftBlinker==true)
				{
					textMeshProUGUI.text = "Проверете страничното си огледало. Ако не виждате зад вас превозно средство, което ще предприеме изпреварване, отсрещната лента е свободна и линията между платната е прекъсната, преминете в лявата лента на платното.";
					currentStep = StepState.Step4;
				}
				break;

			case StepState.Step4:
				{
					if(cubeLeft)
					{
						textMeshProUGUI.text = "Пуснете десния си мигач.";
						currentStep = StepState.Step5;
					}
				}
				break;
			case StepState.Step5:
				{
					if (cubeLeft && carController.isRightBlinker == true)
					{
						textMeshProUGUI.text = "Приберете се в дясната лента, като спазвате безопасна дистанция с колата зад вас.";
						currentStep = StepState.Step6;
					}
				}
				break;
			case StepState.Step6:
				{
					if(cubeRight)
					{
						currentStep = StepState.Completed;
					}
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
		textMeshProUGUI.text = "Съединителят е махнат плавно.";
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
	void SetupRestartButtonAnotherCar()
	{
		if (restartButtonAnotherCarPanel != null)
		{
			restartButtonAnotherCarPanel.onClick.RemoveAllListeners();
			restartButtonAnotherCarPanel.onClick.AddListener(RestartLevel);
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

