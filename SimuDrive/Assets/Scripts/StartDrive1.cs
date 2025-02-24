using TMPro;
using UnityEngine;
using System.Collections;

public class StartDrive1 : MonoBehaviour
{
    [SerializeField] private CarController carController; // Reference to the CarController
    [SerializeField] private TextMeshProUGUI textMeshProUGUI; // Reference to the Text UI
    [SerializeField] private GameObject panelProblem;
    [SerializeField] private GameObject panelDone;

    private float clutchInput;
    private float acceleratorInput;
    private float previousClutchInput; // Track previous clutch input
    private float clutchReleaseThreshold = 0.5f; // Threshold for smooth release

    private enum StepState { Step1, Step2, Step3, Step4, Completed }
    private StepState currentStep = StepState.Step1;

    void Start()
    {
        panelProblem.SetActive(false);
        panelDone.SetActive(false);
        textMeshProUGUI.text = "Натисни съединителя. (Най-левия педал)";
    }

    void Update()
    {
        clutchInput = Input.GetAxis("Clutch");
        acceleratorInput = Input.GetAxis("Accelerator");

        // Debugging logs
        Debug.Log("Clutch Input: " + clutchInput);
        Debug.Log("Car Speed: " + carController.Speed);
        Debug.Log("Current Step: " + currentStep);

        previousClutchInput = clutchInput; // Update previous clutch input each frame

        // Warning message for clutch
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

        // Step-based instructions
        switch (currentStep)
        {
            case StepState.Step1:
                while (true)
                {
                    if (clutchInput > 0.5f || Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        textMeshProUGUI.text = "Превключете на първа скорост, като придърпате скоростния лост към тялото Ви и след това нагоре.";
                        currentStep = StepState.Step2;
                        break;
                    }
                    else
                    {
                        textMeshProUGUI.text = "Натисни съединителя. (Най-левия педал)";
                    }
                }

                break;


            case StepState.Step2:
                if ((carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1)) && Time.timeScale != 0f)
                {
                    textMeshProUGUI.text = "Подайте леко и внимателно газ. (Най-десния педал)";
                    currentStep = StepState.Step3;
                    Debug.Log("Step 2 Complete - First Gear Engaged.");
                }
                break;

            case StepState.Step3:
                if ((acceleratorInput > 0.1f || Input.GetKeyDown(KeyCode.UpArrow)) && Time.timeScale != 0f)
                {
                    textMeshProUGUI.text = "Когато колата потегли, внимателно и плавно вдигнете крака си от педала на съединителя.";
                    currentStep = StepState.Step4;
                    Debug.Log("Step 3 Complete - Accelerating.");
                }
                break;

            case StepState.Step4:
                if (((clutchInput < 0.2f || Input.GetKeyUp(KeyCode.LeftShift)) && carController.Speed > 5f) && Time.timeScale != 0f)
                {
                    textMeshProUGUI.text = "Съединителят е махнат плавно.";
                    currentStep = StepState.Completed;
                    StartCoroutine(ShowCompletionPanel());
                }
                break;
        }
    }

    private IEnumerator ShowCompletionPanel()
    {
        yield return new WaitForSeconds(5f);
        panelDone.SetActive(true);
        Time.timeScale = 0f; // Pause the game
        Debug.Log("All steps completed successfully!");
    }

    // Check if clutch is released smoothly
    private bool IsClutchReleasedSmoothly()
    {
        float clutchReleaseRate = previousClutchInput - clutchInput;
        return clutchReleaseRate <= clutchReleaseThreshold;
    }
}
