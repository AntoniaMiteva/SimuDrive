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
    [SerializeField] private GameObject panelDone;
    [SerializeField] private GameObject panelInstruction;
    [SerializeField] private Button backButton;

    private float clutchInput;
    private float acceleratorInput;
    private float brakeInput;
    private bool levelCompleted = false;

    private enum StepState { Step1, Step2, Step3, Step4, Step5, Completed }
    private StepState currentStep = StepState.Step1;

    private bool isBrakePressed = false;
    private bool clutchWasPressed = false;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        panelProblem.SetActive(false);
        panelProblem2.SetActive(false);
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
    }

    void Update()
    {
        if (!levelCompleted)
        {
            clutchInput = Input.GetAxis("Clutch");
            acceleratorInput = Input.GetAxis("Accelerator");
            brakeInput = Input.GetAxis("Brake");

            isBrakePressed = brakeInput > 0.1f || Input.GetKey(KeyCode.Space);

            // Check if clutch was pressed and then released too early
            if (currentStep >= StepState.Step3 && clutchWasPressed &&
                !(clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift)) &&
                carController.Speed <= 0.1f)
            {
                panelProblem.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                panelProblem.SetActive(false);
                Time.timeScale = 1f;
            }

            ProcessSteps();
        }
    }

    private void ProcessSteps()
    {
        switch (currentStep)
        {
            case StepState.Step1:
                if (isBrakePressed)
                {
                    textMeshProUGUI.text = "Освободете ръчната спирачка (B).";
                    currentStep = StepState.Step2;
                }
                else
                {
                    textMeshProUGUI.text = "Натиснете спирачката (Space).";
                }
                break;

            case StepState.Step2:
                if (!isBrakePressed && panelDone.activeSelf == false)
                {
                    panelProblem2.SetActive(true);
                    Time.timeScale = 0f;
                }
                else if (Input.GetKeyUp(KeyCode.B))
                {
                    panelProblem2.SetActive(false);
                    Time.timeScale = 1f;
                    textMeshProUGUI.text = "Натиснете съединителя (Left Shift).";
                    currentStep = StepState.Step3;
                }
                else
                {
                    panelProblem2.SetActive(false);
                    Time.timeScale = 1f;
                    textMeshProUGUI.text = "Освободете ръчната спирачка (B) докато държите спирачката (Space).";
                }
                break;

            case StepState.Step3:
                if (!isBrakePressed && panelDone.activeSelf == false)
                {
                    panelProblem2.SetActive(true);
                    Time.timeScale = 0f;
                }
                else if (clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift))
                {
                    clutchWasPressed = true;
                    panelProblem2.SetActive(false);
                    Time.timeScale = 1f;
                    textMeshProUGUI.text = "Превключете на първа скорост (1).";
                    currentStep = StepState.Step4;
                }
                else
                {
                    panelProblem2.SetActive(false);
                    Time.timeScale = 1f;
                    textMeshProUGUI.text = "Натиснете съединителя (Left Shift).";
                }
                break;

            case StepState.Step4:
                if (!isBrakePressed && panelDone.activeSelf == false)
                {
                    panelProblem2.SetActive(true);
                    Time.timeScale = 0f;
                }
                else if (carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1))
                {
                    panelProblem2.SetActive(false);
                    Time.timeScale = 1f;
                    textMeshProUGUI.text = "Подайте газ (Up Arrow) и започнете да отпускате съединителя (Left Shift).";
                    currentStep = StepState.Step5;
                }
                else
                {
                    panelProblem2.SetActive(false);
                    Time.timeScale = 1f;
                    textMeshProUGUI.text = "Превключете на първа скорост (1).";
                }
                break;

            case StepState.Step5:
                if (!isBrakePressed && panelDone.activeSelf == false)
                {
                    panelProblem2.SetActive(true);
                    Time.timeScale = 0f;
                }
                else if ((acceleratorInput > 0.1f || Input.GetKey(KeyCode.UpArrow)) &&
                    (clutchInput < 0.2f || !Input.GetKey(KeyCode.LeftShift)))
                {
                    panelProblem2.SetActive(false);
                    Time.timeScale = 1f;
                    CompleteLevel();
                    panelInstruction.SetActive(false);
                }
                else
                {
                    panelProblem2.SetActive(false);
                    Time.timeScale = 1f;
                    textMeshProUGUI.text = "Подайте газ (Up Arrow) и започнете да отпускате съединителя (Left Shift).";
                }
                break;
        }
    }

    private void CompleteLevel()
    {
        levelCompleted = true;
        textMeshProUGUI.text = "Съединителят е освободен плавно и колата потегли.";
        carController.enabled = false;
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        StartCoroutine(ShowCompletionPanel());
    }

    private System.Collections.IEnumerator ShowCompletionPanel()
    {
        yield return new WaitForSeconds(5f);
        panelDone.SetActive(true);
        panelProblem.SetActive(false);
        panelProblem2.SetActive(false);
        textMeshProUGUI.text = "";

        if (backButton != null)
        {
            backButton.interactable = true;
        }
    }

    public void BackToStart()
    {
        ManualBackToStart();
    }

    public void ManualBackToStart()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Drive Menu");
    }
}