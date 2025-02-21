using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDrive1 : MonoBehaviour
{
    [SerializeField] private GameObject step1;
    [SerializeField] private GameObject step2;
    [SerializeField] private GameObject step3;
    [SerializeField] private GameObject step4;
    [SerializeField] private GameObject stepClutchProblem;
    [SerializeField] private CarController carController; // Reference to the CarController

    private float clutchInput;
    private float acceleratorInput;

    private float previousClutchInput; // Track previous clutch input for smooth release check
    private float clutchReleaseThreshold = 0.5f; // Threshold for smooth release

    private enum StepState { Step1, Step2, Step3, Step4, Completed }
    private StepState currentStep = StepState.Step1;

    void Start()
    {
        step1.SetActive(true);
        step2.SetActive(false);
        step3.SetActive(false);
        step4.SetActive(false);
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
                if (acceleratorInput > 0.1f || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    step3.SetActive(false);
                    step4.SetActive(true);
                    currentStep = StepState.Step4;
                }
                break;

            case StepState.Step4:
                // Check if clutch is almost fully released and speed is above 5
                if ((clutchInput < 0.2f || Input.GetKeyUp(KeyCode.LeftShift)) && carController.Speed > 5f) // Speed above 5
                {
                    if (IsClutchReleasedSmoothly())
                    {
                        // Step 4 completed successfully
                        step4.SetActive(false);
                        currentStep = StepState.Completed;
                        Debug.Log("Step 4 completed: Clutch released smoothly and speed is above 5!");
                    }
                    else
                    {
                        // Clutch was released too quickly, show a warning
                        Debug.LogWarning("Release the clutch more carefully!");
                    }
                }
                else if (carController.Speed <= 5f)
                {
                    // Car speed is not above 5, show a warning
                    Debug.LogWarning("Car speed must be above 5 to complete Step 4.");
                }
                break;

            case StepState.Completed:
                // All steps completed
                break;
        }
    }




    // Helper method to check if the clutch is released smoothly
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