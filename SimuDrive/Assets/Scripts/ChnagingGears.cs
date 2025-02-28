using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangingGears : MonoBehaviour
{
    [SerializeField] private CarController carController;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject panelProblem;
    [SerializeField] private GameObject panelDone;
    [SerializeField] private GameObject panelInstruction;
    [SerializeField] private Button backButton;


    private float clutchInput;
    private float acceleratorInput;
    private float previousClutchInput;
    private bool levelCompleted = false;
    private char gear = '0';

    private enum StepState { Step1, Step2, Step3, Step4, Step5, Step6, Step7, Step8, Step9, Step10, Completed }
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
                if (carController.Speed <= 5f && gear != '0' && !(clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift)))
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
                if (clutchInput > 0.8f || Input.GetKeyDown(KeyCode.LeftShift))
                {
                    textMeshProUGUI.text = "Превключете на ПЪРВА скорост.";
                    currentStep = StepState.Step2;
                }
                else
                {
                    textMeshProUGUI.text = "Натиснете съединителя до долу.";
                }
                break;

            case StepState.Step2:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        textMeshProUGUI.text = "Превключете на ВТОРА скорост.";
                        gear = '1';
                        currentStep = StepState.Step3;
                    }
                }
                break;

            case StepState.Step3:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 2 || Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        textMeshProUGUI.text = "Превключете на ТРЕТА скорост.";
                        gear = '2';
                        currentStep = StepState.Step4;
                    }
                }
                break;

            case StepState.Step4:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 3 || Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        textMeshProUGUI.text = "Превключете на ЧЕТВЪРТА скорост.";
                        gear = '3';
                        currentStep = StepState.Step5;
                    }
                }
                break;

            case StepState.Step5:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 4 || Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        textMeshProUGUI.text = "Превключете на ПЕТА скорост.";
                        gear = '4';
                        currentStep = StepState.Step6;
                    }
                }
                break;
            case StepState.Step6:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 5 || Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        textMeshProUGUI.text = "Превключете на ЗАДНА скорост.";
                        gear = '5';
                        currentStep = StepState.Step7;
                    }
                }
                break;
            case StepState.Step7:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == -1 || Input.GetKeyDown(KeyCode.R))
                    {
                        textMeshProUGUI.text = "Освободете от скорост.";
                        gear = 'R';
                        currentStep = StepState.Step8;
                    }
                }
                break;
            case StepState.Step8:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 0 || Input.GetKeyDown(KeyCode.Alpha0))
                    {
                        gear = '0';
                        textMeshProUGUI.text = "Освободете съединителя.";
                        currentStep = StepState.Step9;
                    }
                }
                break;
            case StepState.Step9:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    textMeshProUGUI.text = "Освободете съединителя.";
                }
                else
                {
                    CompleteLevel();
                }

                break;
            
        }
    }


    private void CompleteLevel()
    {
        levelCompleted = true;
        textMeshProUGUI.text = "Съединителят е махнат плавно.";
        carController.enabled = false;
        panelDone.SetActive(true);
        panelInstruction.SetActive(false);
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

}