using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Parking1 : MonoBehaviour
{
    [SerializeField] private CarController carController;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject panelProblem;
    [SerializeField] private GameObject panelDone;
    [SerializeField] private GameObject panelInstruction;
    [SerializeField] private Button backButton;
	[SerializeField] private GameObject panelProblemCollision;
	[SerializeField] private Button quitButton;
	[SerializeField] private GameObject panelQuit;
	[SerializeField] private Button restartButton;
	[SerializeField] private Button buttonAnotherCar;
	[SerializeField] private GameObject panelAnotherCar;

	private float clutchInput;
    private float acceleratorInput;
    private float previousClutchInput;
    private bool levelCompleted = false;

    private enum StepState { Step0, Step1, Step2, Step3, Step4, Step5, Step6, Completed }
    private StepState currentStep = StepState.Step0;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        panelProblem.SetActive(false);
        panelDone.SetActive(false);
		panelQuit.SetActive(false);
        panelAnotherCar.SetActive(false);
		panelInstruction.SetActive(true);

		SetupRestartButton();
        SetupRestartButtonAnotherCar();

		textMeshProUGUI.text = "Пусни аварийните светлини.";

        // Check if EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // Setup button manually - don't rely on Inspector
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ManualBackToStart);
        }
        else
        {

            Button[] buttons = panelDone.GetComponentsInChildren<Button>(true);
            if (buttons.Length > 0)
            {
                backButton = buttons[0];
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(ManualBackToStart);
            }
        }
    }

    void Update()
    {
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
            panelDone.SetActive(false);
            panelProblem.SetActive(false);
		}


	}

    private void ProcessSteps()
    {
        switch (currentStep)
        {
			case StepState.Step0:
				if (carController.isEmergency)
                {
					textMeshProUGUI.text = "Трябва да паркираш между черния и белия автомобил от дясната ти страна. Включи на първа скорост.";
					currentStep = StepState.Step1;
				}
				else
                {
					textMeshProUGUI.text = "Пусни аварийните светлини.";
				}

				break;
			case StepState.Step1:
				if ((carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1)) && Time.timeScale != 0f)
				{
					textMeshProUGUI.text = "Изтегли бавно колата напред, докато рамото ти се изравни центъра на жълтия автомобил.";
					currentStep = StepState.Step2;
                }
				break;

            case StepState.Step2:
                if (carController.carParkChecker1)
                {
                    textMeshProUGUI.text = "Спрете автомобила и включете на задна скорост.";
                    currentStep = StepState.Step3;
                }
                break;

            case StepState.Step3:
				if ((carController.Gear == -1 || Input.GetKeyDown(KeyCode.R)) && Time.timeScale != 0f)
				{
					textMeshProUGUI.text = "Завъртете волана до край надясно и започнете леко да давате назад, докато станете успоредни с другите автомобили.";
					currentStep = StepState.Step4;
				}
				break;

            case StepState.Step4:
				if (carController.Rotation < 0f)
				{
					textMeshProUGUI.text = "Изправете волана и дайте бавно и внивателно назад.";
					currentStep = StepState.Step5;
				}
				break;

			case StepState.Step5:
				if (carController.carParkChecker2)
				{
					textMeshProUGUI.text = "Спрете автомобила.";
					currentStep = StepState.Step6;
				}
				break;

			case StepState.Step6:
				if (carController.Speed<1f)
				{
					textMeshProUGUI.text = "Успешно паркирахте.";
					CompleteLevel();
				}
				break;
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
	private void CompleteLevel()
    {
        Debug.Log("Level completed - showing completion panel");

        // Mark level as completed
        levelCompleted = true;

        // Update text and disable car controller
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
}