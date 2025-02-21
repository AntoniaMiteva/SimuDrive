using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDrive2 : MonoBehaviour
{
    [SerializeField] private GameObject step1;
    [SerializeField] private GameObject step2;
    [SerializeField] private GameObject step3;
    [SerializeField] private GameObject step4;
    [SerializeField] private GameObject stepClutchProblem;
    [SerializeField] private GameObject stepSlopeWarning;
    [SerializeField] private CarController carController;

    private float clutchInput;
    private float acceleratorInput;
    private float previousClutchInput = 1.0f;
    private float clutchReleaseThreshold = 0.5f;

    private enum StepState { Step1, Step2, Step3, Step4, Completed }
    private StepState currentStep = StepState.Step1;

    private bool isRollingBackward = false;

    [SerializeField] private Transform carTransform;
    [SerializeField] private float slopeThreshold = 5f;
    public bool isOnSlope = false;
    private float slopeAngle = 0f;

    void Start()
    {
        step1.SetActive(true);
        step2.SetActive(false);
        step3.SetActive(false);
        step4.SetActive(false);
        stepClutchProblem.SetActive(false);
        stepSlopeWarning.SetActive(false);
    }

    void Update()
    {
        CheckSlope();
        clutchInput = Input.GetAxis("Clutch");
        acceleratorInput = Input.GetAxis("Accelerator");

        if (isOnSlope && IsRollingBackward())
        {
            isRollingBackward = true;
            stepSlopeWarning.SetActive(true);
            Debug.LogWarning("You're rolling backward! Use the brake or handbrake.");
        }
        else
        {
            isRollingBackward = false;
            stepSlopeWarning.SetActive(false);
        }

        if (carController.Speed <= 5f && carController.Gear == 1 && !(clutchInput >= 0.2f || Input.GetKey(KeyCode.LeftShift)))
        {
            stepClutchProblem.SetActive(true);
        }
        else
        {
            stepClutchProblem.SetActive(false);
        }

        switch (currentStep)
        {
            case StepState.Step1:
                if (isOnSlope && !Input.GetKey(KeyCode.Space))
                {
                    Debug.LogWarning("Use the handbrake (Space) before starting on a hill!");
                    return;
                }
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
                        carController.motorForce = isOnSlope ? 2000 : 1000;
                        step2.SetActive(false);
                        step3.SetActive(true);
                        currentStep = StepState.Step3;
                    }
                }
                break;

            case StepState.Step3:
                float requiredAcceleration = isOnSlope ? Mathf.Clamp(slopeAngle / 10f, 0.5f, 1f) : 0.1f;
                if (acceleratorInput > requiredAcceleration || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    step3.SetActive(false);
                    step4.SetActive(true);
                    currentStep = StepState.Step4;
                }
                break;

            case StepState.Step4:
                if ((clutchInput < 0.2f || Input.GetKeyUp(KeyCode.LeftShift)) && carController.Speed > 8f)
                {
                    if (IsClutchReleasedSmoothly())
                    {
                        step4.SetActive(false);
                        currentStep = StepState.Completed;
                        Debug.Log("Step 4 completed: Clutch released smoothly and speed is above 8!");
                    }
                    else
                    {
                        Debug.LogWarning("Release the clutch more carefully!");
                    }
                }
                else if (carController.Speed <= 8f)
                {
                    Debug.LogWarning("Car speed must be above 8 to complete Step 4.");
                }
                break;
        }
    }

    private void CheckSlope()
    {
        RaycastHit hit;
        if (Physics.Raycast(carTransform.position, Vector3.down, out hit, 1.5f))
        {
            Debug.DrawLine(carTransform.position, hit.point, Color.red);
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            isOnSlope = slopeAngle > slopeThreshold;
            Debug.Log("Slope Angle: " + slopeAngle);
        }
        else
        {
            isOnSlope = false;
        }
    }

    private bool IsRollingBackward()
    {
        Vector3 localVelocity = carTransform.InverseTransformDirection(carController.GetComponent<Rigidbody>().linearVelocity);
        return localVelocity.z < -0.5f;
    }

    private bool IsClutchReleasedSmoothly()
    {
        float clutchReleaseRate = previousClutchInput - clutchInput;
        previousClutchInput = clutchInput;
        return clutchReleaseRate <= clutchReleaseThreshold;
    }
}