using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangingGears : MonoBehaviour
{
    [SerializeField] private GameObject step1;
    [SerializeField] private GameObject step2;
    [SerializeField] private GameObject step3;
    [SerializeField] private GameObject step4;
    [SerializeField] private GameObject step5;
    [SerializeField] private GameObject step6;
    [SerializeField] private GameObject step7;
    [SerializeField] private GameObject step8;
    [SerializeField] private GameObject step9;
    [SerializeField] private GameObject step10;
    [SerializeField] private GameObject stepClutchProblem;
    [SerializeField] private CarController carController; // Reference to the CarController

    private float clutchInput;
    private float acceleratorInput;

    private float previousClutchInput; // Track previous clutch input for smooth release check
    private float clutchReleaseThreshold = 0.5f; // Threshold for smooth release

    private enum StepState { Step1, Step2, Step3, Step4, Step5, Step6, Step7, Step8, Step9, Step10, Completed }
    private StepState currentStep = StepState.Step1;

    void Start()
    {
        step1.SetActive(true);
        step2.SetActive(false);
        step3.SetActive(false);
        step4.SetActive(false);
        step5.SetActive(false);
        step6.SetActive(false);
        step7.SetActive(false);
        step8.SetActive(false);
        step9.SetActive(false);
        step10.SetActive(false);
        stepClutchProblem.SetActive(false);
    }

    void Update()
    {
        clutchInput = Input.GetAxis("Clutch");
        acceleratorInput = Input.GetAxis("Accelerator");

        // Debugging logs
        Debug.Log("Clutch Input: " + clutchInput);
        Debug.Log("Car Speed: " + carController.Speed);

        if (carController.Speed <= 5f && carController.Gear == 1 && !(clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift)))
        {
            stepClutchProblem.SetActive(true); // Show warning
        }
        else
        {
            stepClutchProblem.SetActive(false); // Hide warning
        }


        switch (currentStep)
        {
            case StepState.Step1:
                if (clutchInput > 0.8f || Input.GetKeyDown(KeyCode.LeftShift))
                {
                    step1.SetActive(false);
                    step2.SetActive(true);
                    currentStep = StepState.Step2;
                }
                break;

            case StepState.Step2:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 1 || Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        step2.SetActive(false);
                        step3.SetActive(true);
                        currentStep = StepState.Step3;
                    }
                }
                break;

            case StepState.Step3:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 2 || Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        step3.SetActive(false);
                        step4.SetActive(true);
                        currentStep = StepState.Step4;
                    }
                }
                break;

            case StepState.Step4:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 3 || Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        step4.SetActive(false);
                        step5.SetActive(true);
                        currentStep = StepState.Step5;
                    }
                }
                break;

            case StepState.Step5:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 4 || Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        step5.SetActive(false);
                        step6.SetActive(true);
                        currentStep = StepState.Step6;
                    }
                }
                break;
            case StepState.Step6:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 5 || Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        step6.SetActive(false);
                        step7.SetActive(true);
                        currentStep = StepState.Step7;
                    }
                }
                break;
            case StepState.Step7:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == -1 || Input.GetKeyDown(KeyCode.R))
                    {
                        step7.SetActive(false);
                        step8.SetActive(true);
                        currentStep = StepState.Step8;
                    }
                }
                break;
            case StepState.Step8:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    if (carController.Gear == 0 || Input.GetKeyDown(KeyCode.Alpha0))
                    {
                        step8.SetActive(false);
                        step9.SetActive(true);
                        currentStep = StepState.Step9;
                    }
                }
                break;
            case StepState.Step9:
                if (clutchInput > 0.8f || Input.GetKey(KeyCode.LeftShift))
                {
                    step9.SetActive(true);
                }
                else
                {
                    // Step 4 completed successfully
                    step9.SetActive(false);
                    step10.SetActive(true);
                    currentStep = StepState.Completed;
                }

                break;
            
            case StepState.Completed:
                step1.SetActive(false);
                step2.SetActive(false);
                step3.SetActive(false);
                step4.SetActive(false);
                step5.SetActive(false);
                step6.SetActive(false);
                step7.SetActive(false);
                step8.SetActive(false);
                step9.SetActive(false);
                step10.SetActive(true);
                stepClutchProblem.SetActive(false);
                break;
        }
    }

    private bool IsClutchReleasedSmoothly()
    {
        // Calculate the rate of clutch release
        float clutchReleaseRate = previousClutchInput - clutchInput;

        // Update the previous clutch input for the next frame
        previousClutchInput = clutchInput;

        // If the release rate is too high, the clutch was released too quickly
        if (clutchReleaseRate > clutchReleaseThreshold)
        {
            return false; // Clutch was released too quickly
        }

        return true; // Clutch was released smoothly
    }

}