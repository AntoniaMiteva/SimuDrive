using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartDrive1 : MonoBehaviour
{
    [SerializeField] private CarController carController;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject panelProblem;
    [SerializeField] private GameObject panelDone;
    [SerializeField] private GameObject panelInstruction;
    [SerializeField] private Button backButton; // Reference to your button

    private float clutchInput;
    private float acceleratorInput;
    private float previousClutchInput;
    private bool levelCompleted = false;

    private enum StepState { Step1, Step2, Step3, Step4, Completed }
    private StepState currentStep = StepState.Step1;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        panelProblem.SetActive(false);
        panelDone.SetActive(false);
        panelInstruction.SetActive(true);

        
        textMeshProUGUI.text = "Натиснете съединителя до долу.";

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

    }

    private void ProcessSteps()
    {
        switch (currentStep)
        {
            case StepState.Step1:
                if (clutchInput > 0.5f || Input.GetKey(KeyCode.LeftShift))
                {
                    textMeshProUGUI.text = "Превключете на първа скорост, като придърпате скоростния лост към тялото Ви и след това нагоре.";
                    currentStep = StepState.Step2;
                }
                else
                {
                    textMeshProUGUI.text = "Натиснете съединителя до долу.";
                }
                break;

            case StepState.Step2:
                if ((carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1)) && Time.timeScale != 0f)
                {
                    textMeshProUGUI.text = "Подайте леко и внимателно газ.";
                    currentStep = StepState.Step3;
                }
                break;

            case StepState.Step3:
                if ((acceleratorInput > 0.1f || Input.GetKeyDown(KeyCode.UpArrow)) && Time.timeScale != 0f)
                {
                    textMeshProUGUI.text = "Когато колата потегли, внимателно и плавно вдигнете крака си от педала на съединителя.";
                    currentStep = StepState.Step4;
                }
                break;

            case StepState.Step4:
                if (carController.Speed > 2f)
                {
                    if ((carController.isSteeringWheelConnected && clutchInput < 0.2f) || Input.GetKeyUp(KeyCode.LeftShift))
                    {
                        
                        CompleteLevel();
                        panelInstruction.SetActive(false);
                    }
                    else
                    {
                        textMeshProUGUI.text = "Когато колата потегли, внимателно и плавно вдигнете крака си от педала на съединителя.";
                    }
                }
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
}