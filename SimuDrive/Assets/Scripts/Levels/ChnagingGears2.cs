using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangingGears2 : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private CarController carController;
	[SerializeField] private TextMeshProUGUI textMeshProUGUI;
	[SerializeField] private GameObject panelProblem;
	[SerializeField] private GameObject panelProblemSpeed;
	[SerializeField] private GameObject panelDone;
	[SerializeField] private GameObject panelInstruction;
	[SerializeField] private Button backButton;

	[Header("Settings")]
	[SerializeField] private float minSpeedForSecondGear = 10f;
	[SerializeField] private float minSpeedForThirdGear = 30f;

	// Private variables
	private float clutchInput;
	private float acceleratorInput;
	private bool levelCompleted = false;
	private char currentGear = '0';
	private bool speedNotEnough = false;
	private bool showingSpeedPanel = false;

	private enum StepState
	{
		Step1,  // Press clutch
		Step2,  // Shift to 1st
		Step3,  // Accelerate and shift to 2nd
		Step4,  // Accelerate and shift to 3rd
		Completed
	}
	private StepState currentStep = StepState.Step1;

	void Start()
	{
		InitializeUI();
		SetupEventSystem();
		SetupBackButton();
	}

	void InitializeUI()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;

		panelProblem.SetActive(false);
		panelProblemSpeed.SetActive(false);
		panelDone.SetActive(false);
		panelInstruction.SetActive(true);

		textMeshProUGUI.text = "Натиснете съединителя до долу.";
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

	void Update()
	{
		if (levelCompleted) return;

		GetInputs();
		HandlePanels();
		ProcessSteps();
	}

	void GetInputs()
	{
		clutchInput = Input.GetAxis("Clutch");
		acceleratorInput = Input.GetAxis("Accelerator");
	}

	void HandlePanels()
	{
		// Handle gear problem panel (stalling)
		bool shouldShowGearProblem = currentGear != '0' &&
								   carController.Speed <= 5f &&
								   clutchInput < 0.2f &&
								   !Input.GetKey(KeyCode.LeftShift);

		panelProblem.SetActive(shouldShowGearProblem);
		Time.timeScale = shouldShowGearProblem ? 0f : 1f;

		// Handle speed problem panel
		if (speedNotEnough && !showingSpeedPanel)
		{
			showingSpeedPanel = true;
			panelProblemSpeed.SetActive(true);
			Time.timeScale = 0f;
		}

		// Check for accelerator input to dismiss speed panel
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
				HandleStep1();
				break;

			case StepState.Step2:
				HandleStep2();
				break;

			case StepState.Step3:
				HandleStep3();
				break;

			case StepState.Step4:
				HandleStep4();
				break;

			case StepState.Completed:
				CompleteLevel();
				break;
		}
	}

	void HandleStep1()
	{
		if (clutchInput > 0.5f || Input.GetKey(KeyCode.LeftShift))
		{
			textMeshProUGUI.text = "Превключете на първа скорост.";
			currentStep = StepState.Step2;
		}
		else
		{
			textMeshProUGUI.text = "Натиснете съединителя до долу.";
		}
	}

	void HandleStep2()
	{
		if ((carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1)) && Time.timeScale != 0f)
		{
			currentGear = '1';
			textMeshProUGUI.text = $"Наберете скорост ({minSpeedForSecondGear}км) и сменете на втора скорост.";
			currentStep = StepState.Step3;
		}
	}

	void HandleStep3()
	{
		if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
		{
			if (carController.Gear == 2 || Input.GetKeyDown(KeyCode.Alpha2))
			{
				if (carController.Speed >= minSpeedForSecondGear)
				{
					currentGear = '2';
					textMeshProUGUI.text = $"Наберете скорост ({minSpeedForThirdGear}км) и сменете на трета скорост.";
					currentStep = StepState.Step4;
				}
				else
				{
					speedNotEnough = true;
				}
			}
		}
	}

	void HandleStep4()
	{
		if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
		{
			if (carController.Gear == 3 || Input.GetKeyDown(KeyCode.Alpha3))
			{
				if (carController.Speed >= minSpeedForThirdGear)
				{
					currentStep = StepState.Completed;
				}
				else
				{
					speedNotEnough = true;
				}
			}
		}
	}

	void CompleteLevel()
	{
		levelCompleted = true;
		textMeshProUGUI.text = "Успешно завършихте упражнението!";
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
}