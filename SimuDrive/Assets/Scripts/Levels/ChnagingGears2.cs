using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangingGears2 : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private CarController carController;
	[SerializeField] private TextMeshProUGUI textMeshProUGUI;
	[SerializeField] private GameObject panelProblem;
	[SerializeField] private GameObject panelProblemSpeed;
	[SerializeField] private GameObject panelProblem3;
	[SerializeField] private GameObject panelDone;
	[SerializeField] private GameObject panelInstruction;
	[SerializeField] private Button backButton;
	[SerializeField] private Button restartButton;

	[Header("Settings")]
	[SerializeField] private float minSpeedForSecondGear = 10f;
	[SerializeField] private float minSpeedForThirdGear = 20f;
	[SerializeField] private float minSpeedForFourthGear = 30f;
	[SerializeField] private float minSpeedForFifthGear = 40f;

	private float clutchInput;
	private float acceleratorInput;
	private bool levelCompleted = false;
	private bool speedNotEnough = false;
	private bool showingSpeedPanel = false;

	private enum StepState
	{
		Step1, Step2, Step3, Step4, Step5, Step6, Step7, Completed
	}
	private StepState currentStep = StepState.Step1;

	void Start()
	{
		InitializeUI();
		SetupEventSystem();
		SetupBackButton();
		SetupRestartButton();
	}

	void InitializeUI()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		panelProblem.SetActive(false);
		panelProblemSpeed.SetActive(false);
		panelProblem3.SetActive(false); 
		panelDone.SetActive(false);
		panelInstruction.SetActive(true);

		textMeshProUGUI.text = "��������� ����������� �� ����.";
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

	void SetupRestartButton()
	{
		if (restartButton != null)
		{
			restartButton.onClick.RemoveAllListeners();
			restartButton.onClick.AddListener(RestartLevel);
		}
	}

	void Update()
	{
		if (levelCompleted) return;

		GetInputs();
		HandlePanels();
		ProcessSteps();

		if(carController.carCollided)
		{
			panelProblem3.SetActive(true);
			panelProblem.SetActive(false);
			panelProblemSpeed.SetActive(false);
			carController.enabled = false;
			Time.timeScale = 1f;
		}
		else
		{
			panelProblem3.SetActive(false);
			Time.timeScale = 1f;
		}
	}

	void GetInputs()
	{
		clutchInput = Input.GetAxis("Clutch");
		acceleratorInput = Input.GetAxis("Accelerator");
	}

	void HandlePanels()
	{
		bool shouldShowGearProblem = carController.Gear != 0 &&
								   carController.Speed <= 5f &&
								   clutchInput < 0.2f &&
								   !Input.GetKey(KeyCode.LeftShift);

		panelProblem.SetActive(shouldShowGearProblem);
		Time.timeScale = shouldShowGearProblem ? 0f : 1f;

		if (speedNotEnough && !showingSpeedPanel)
		{
			showingSpeedPanel = true;
			panelProblemSpeed.SetActive(true);
			Time.timeScale = 0f;
		}

		if (showingSpeedPanel && (acceleratorInput > 0.1f || Input.GetKeyDown(KeyCode.UpArrow)))
		{
			speedNotEnough = false;
			showingSpeedPanel = false;
			panelProblemSpeed.SetActive(false);
			Time.timeScale = 1f;
		}
	}

	void ProcessSteps()
	{
		switch (currentStep)
		{
			case StepState.Step1:
				if (clutchInput > 0.5f || Input.GetKey(KeyCode.LeftShift))
				{
					textMeshProUGUI.text = "����������� �� ����� �������.";
					currentStep = StepState.Step2;
				}
				break;

			case StepState.Step2:
				if ((carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1)) && Time.timeScale != 0f)
				{
					textMeshProUGUI.text = $"�������� ������� ({minSpeedForSecondGear}��) � ������� �� ����� �������.";
					currentStep = StepState.Step3;
				}
				break;

			case StepState.Step3:
				if ((clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift)) &&
					(carController.Gear == 2 || Input.GetKeyDown(KeyCode.Alpha2)))
				{
					if (carController.Speed >= minSpeedForSecondGear)
					{
						textMeshProUGUI.text = $"�������� ������� ({minSpeedForThirdGear}��) � ������� �� ����� �������.";
						currentStep = StepState.Step4;
					}
					else speedNotEnough = true;
				}
				break;

			case StepState.Step4:
				if ((clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift)) &&
					(carController.Gear == 3 || Input.GetKeyDown(KeyCode.Alpha3)))
				{
					if (carController.Speed >= minSpeedForThirdGear)
					{
						textMeshProUGUI.text = $"�������� ������� ({minSpeedForFourthGear}��) � ������� �� �������� �������.";
						currentStep = StepState.Step5;
					}
					else speedNotEnough = true;
				}
				break;

			case StepState.Step5:
				if ((clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift)) &&
					(carController.Gear == 4 || Input.GetKeyDown(KeyCode.Alpha4)))
				{
					if (carController.Speed >= minSpeedForFourthGear)
					{
						textMeshProUGUI.text = $"�������� ������� ({minSpeedForFifthGear}��) � ������� �� ���� �������.";
						currentStep = StepState.Step6;
					}
					else speedNotEnough = true;
				}
				break;

			case StepState.Step6:
				if ((clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift)) &&
					(carController.Gear == 5 || Input.GetKeyDown(KeyCode.Alpha5)))
				{
					if (carController.Speed >= minSpeedForFourthGear)
					{
						textMeshProUGUI.text = "�������� ���� ������� �� 3 �������.";
						currentStep = StepState.Step7;
						StartCoroutine(HandleStep7());
					}
					else speedNotEnough = true;
				}
				break;

			case StepState.Completed:
				CompleteLevel();
				break;
		}
	}

	IEnumerator HandleStep7()
	{
		float timer = 0f;

		while (timer < 3f)
		{
			if (carController.Gear != 5)
			{
				textMeshProUGUI.text = "������ �� ��������� ���� �������!";
				yield break;
			}
			timer += Time.deltaTime;
			yield return null;
		}

		currentStep = StepState.Completed;
	}

	void CompleteLevel()
	{
		levelCompleted = true;
		textMeshProUGUI.text = "������� ���������� ������������!";
		carController.enabled = false;

		panelProblem.SetActive(false);
		panelProblemSpeed.SetActive(false);
		panelInstruction.SetActive(false);
		panelDone.SetActive(true);

		Time.timeScale = 1f;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	public void ManualBackToStart()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene("Drive Menu");
	}

	public void RestartLevel()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	
}
