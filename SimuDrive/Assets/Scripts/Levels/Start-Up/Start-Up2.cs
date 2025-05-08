using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartDrive2 : MonoBehaviour
{
    [SerializeField] private CarController carController;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject panelProblem;
    [SerializeField] private GameObject panelProblem2;
    [SerializeField] private GameObject panelProblem3;
    [SerializeField] private GameObject panelDone;
    [SerializeField] private GameObject panelInstruction;
    [SerializeField] private Button backButton;
    [SerializeField] private float maxWaitTime = 2f;
    [SerializeField] private float minMovementSpeed = 0.5f;
	[SerializeField] private GameObject panelQuit;
	[SerializeField] private Button quitButton;

	private float clutchInput;
    private float acceleratorInput;
    private float brakeInput;
    private bool levelCompleted = false;
    private float timeSinceBrakeRelease;
    private bool brakeReleased = false;
    private bool waitingTooLongWarningShown = false;

    private enum StepState
    {
        Step1,          // Press brake
        Step2,          // Release handbrake
        Step3,          // Press clutch
        Step4,          // Shift to 1st gear
        Step5,          // Release brake
        Step6,          // Press accelerator
        Step7,          // Release clutch as car moves
        Completed
    }
    private StepState currentStep = StepState.Step1;

    private bool isBrakePressed = false;
    private bool clutchWasPressed = false;
    private bool acceleratorWasPressed = false;
    private bool carStartedMoving = false;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        panelProblem.SetActive(false);
        panelProblem2.SetActive(false);
        panelProblem3.SetActive(false);
        panelDone.SetActive(false);
        panelInstruction.SetActive(true);

        textMeshProUGUI.text = "Натиснете спирачката (Space).";

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

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

        carController.isManualBrake = true;
    }

    void Update()
    {
        if (panelProblem3.activeSelf)
        {
            return;
        }

        if (carController.isManualBrake)
        {
            carController.ApplyBreaking();
        }

        if (!levelCompleted)
        {
            clutchInput = Input.GetAxis("Clutch");
            acceleratorInput = Input.GetAxis("Accelerator");
            brakeInput = Input.GetAxis("Brake");

            isBrakePressed = brakeInput > 0.1f || Input.GetKey(KeyCode.Space);

            // Auto-hide problem panels when corrected
            if (panelProblem.activeSelf && (clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift)))
            {
                panelProblem.SetActive(false);
            }

            if (panelProblem2.activeSelf && isBrakePressed)
            {
                panelProblem2.SetActive(false);
            }

            if (currentStep == StepState.Step5 && !isBrakePressed && !brakeReleased)
            {
                brakeReleased = true;
                timeSinceBrakeRelease = 0f;
                waitingTooLongWarningShown = false;
            }

            if (brakeReleased && (currentStep == StepState.Step5 || currentStep == StepState.Step6) && !acceleratorWasPressed)
            {
                timeSinceBrakeRelease += Time.deltaTime;
                if (timeSinceBrakeRelease > maxWaitTime && !waitingTooLongWarningShown)
                {
                    panelProblem3.SetActive(true);
                    waitingTooLongWarningShown = true;
                    carController.isManualBrake = true;
                    return;
                }
            }

            if ((currentStep == StepState.Step5 || currentStep == StepState.Step6) && isBrakePressed)
            {
                brakeReleased = false;
                timeSinceBrakeRelease = 0f;
                panelProblem3.SetActive(false);
            }

            if ((currentStep >= StepState.Step3 && currentStep <= StepState.Step7) &&
                clutchWasPressed && !(clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift)) &&
                carController.Speed <= 0.1f)
            {
                panelProblem.SetActive(true);
            }

            carStartedMoving = carController.Speed > minMovementSpeed;

            ProcessSteps();
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
        if (panelProblem3.activeSelf) return;

        switch (currentStep)
        {
            case StepState.Step1:
                if (isBrakePressed)
                {
                    textMeshProUGUI.text = "Освободете ръчната спирачка (B).";
                    currentStep = StepState.Step2;
                }
                break;

            case StepState.Step2:
                if (!isBrakePressed)
                {
                    panelProblem2.SetActive(true);
                    textMeshProUGUI.text = "Трябва да държите спирачката (Space) докато освобождавате ръчната спирачка (B).";
                }
                else if (carController.isManualBrake==false)
                {
                    panelProblem2.SetActive(false);
                    textMeshProUGUI.text = "Натиснете съединителя (Left Shift).";
                    carController.isManualBrake = false;
                    carController.ReleaseBrakes();
                    currentStep = StepState.Step3;
                }
                break;

            case StepState.Step3:
                if (!isBrakePressed)
                {
                    panelProblem2.SetActive(true);
                    textMeshProUGUI.text = "Трябва да държите спирачката (Space) докато натискате съединителя (Left Shift).";
                }
                else if (clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift))
                {
                    clutchWasPressed = true;
                    panelProblem2.SetActive(false);
                    textMeshProUGUI.text = "Превключете на първа скорост (1).";
                    currentStep = StepState.Step4;
                }
                break;

            case StepState.Step4:
                if (!isBrakePressed)
                {
                    panelProblem2.SetActive(true);
                    textMeshProUGUI.text = "Трябва да държите спирачката (Space) докато превключвате скоростта (1).";
                }
                else if (carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1))
                {
                    panelProblem2.SetActive(false);
                    textMeshProUGUI.text = "Освободете спирачката (Space).";
                    currentStep = StepState.Step5;
                }
                break;

            case StepState.Step5:
                if (isBrakePressed)
                {
                    textMeshProUGUI.text = "Трябва да освободите спирачката (Space).";
                    brakeReleased = false;
                    timeSinceBrakeRelease = 0f;
                    waitingTooLongWarningShown = false;
                    panelProblem3.SetActive(false);
                    carController.isManualBrake = true;
                }
                else
                {
                    carController.isManualBrake = false;
                    if (!waitingTooLongWarningShown)
                    {
                        textMeshProUGUI.text = "Натиснете газта (Up Arrow).";
                        currentStep = StepState.Step6;
                    }
                }
                break;

            case StepState.Step6:
                if (acceleratorInput > 0.1f || Input.GetKey(KeyCode.UpArrow))
                {
                    acceleratorWasPressed = true;
                    brakeReleased = false;
                    timeSinceBrakeRelease = 0f;
                    waitingTooLongWarningShown = false;
                    textMeshProUGUI.text = "Започнете да отпускате съединителя (Left Shift) бавно.";
                    currentStep = StepState.Step7;
                    carStartedMoving = false;
                }
                break;

            case StepState.Step7:
                bool isClutchReleased = (clutchInput < 0.3f && !Input.GetKey(KeyCode.LeftShift));

                if (carStartedMoving && isClutchReleased)
                {
                    CompleteLevel();
                    panelInstruction.SetActive(false);
                }
                else if (!carStartedMoving)
                {
                    textMeshProUGUI.text = "Държете газта (Up Arrow) и започнете да отпускате съединителя (Left Shift).";
                }
                else
                {
                    textMeshProUGUI.text = "Продължете да отпускате съединителя (Left Shift) плавно.";
                }
                break;
        }
    }

    private void CompleteLevel()
    {
        StartCoroutine(ShowCompletionPanel());
        levelCompleted = true;
        textMeshProUGUI.text = "Съединителят е освободен плавно и колата потегли успешно.";
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private System.Collections.IEnumerator ShowCompletionPanel()
    {
        yield return new WaitForSeconds(0.5f);
        panelDone.SetActive(true);
        panelProblem.SetActive(false);
        panelProblem2.SetActive(false);
        panelProblem3.SetActive(false);
        textMeshProUGUI.text = "";
        carController.speed = 0;
        carController.enabled = false;
    }

    public void ContinueAfterWaitingTooLong()
    {
        panelProblem3.SetActive(false);
        brakeReleased = false;
        timeSinceBrakeRelease = 0f;
        waitingTooLongWarningShown = false;

        if (currentStep != StepState.Step6)
        {
            currentStep = StepState.Step5;
            textMeshProUGUI.text = "Освободете спирачката (Space).";
        }
        else
        {
            textMeshProUGUI.text = "Натиснете газта (Up Arrow).";
        }
    }

    public void ManualBackToStart()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Drive Menu");
    }

    public void BackToLevel()
    {
        SceneManager.LoadScene("Start-Up-2");
    }
}