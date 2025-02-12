using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float cureenrBreakForce;
    private bool isBreaking;

    private int gear = 0;
    private bool isGear = false; // Gear pedal state
    private bool isStart = false;
    private bool isDrive = false;

    [SerializeField] private Rigidbody carRigidbody; // Assign this in the Inspector
    private float speed;

    [SerializeField] private float motorForce = 0; // This will change with gears
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    [SerializeField] private WheelCollider frontLeftWheelColider;
    [SerializeField] private WheelCollider frontRightWheelColider;
    [SerializeField] private WheelCollider backLeftWheelColider;
    [SerializeField] private WheelCollider backRightWheelColider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform backLeftWheelTransform;
    [SerializeField] private Transform backRightWheelTransform;

    private float timer;
    private float holdDur = 3f;

    [SerializeField] private float maxRPM = 7000f;
    [SerializeField] private float minRPM = 1000f;
    private float currentRPM;


    public InputActionAsset inputActions;
    private InputAction gear1Action;
    private InputAction gear2Action;
    private InputAction gear3Action;
    private InputAction gear4Action;
    private InputAction gear5Action;
    private InputAction gearRAction;

    [SerializeField] private float acceleratorSensitivity = 2f; // Adjust this value to make the pedal more sensitive


    private void Awake()
    {

        // Get the GearShifter action map
        var gearShifterMap = inputActions.FindActionMap("GearShifter");

        // Get the gear1 action
        gear1Action = gearShifterMap.FindAction("gear1");
        gear2Action = gearShifterMap.FindAction("gear2");
        gear3Action = gearShifterMap.FindAction("gear3");
        gear4Action = gearShifterMap.FindAction("gear4");
        gear5Action = gearShifterMap.FindAction("gear5");
        gearRAction = gearShifterMap.FindAction("gearR");

        // Enable the action
        gear1Action.Enable();
        gear2Action.Enable();
        gear3Action.Enable();
        gear4Action.Enable();
        gear5Action.Enable();
        gearRAction.Enable();
        Debug.Log("gear1 Action enabled successfully!");
    }

    private void Start()
    {
        carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0); // Adjust Y to lower
        ConfigureFriction(frontLeftWheelColider);
        ConfigureFriction(frontRightWheelColider);
        ConfigureFriction(backLeftWheelColider);
        ConfigureFriction(backRightWheelColider);
    }

    private void FixedUpdate()
    {
        GetInput();
        UpdateGear(); // Update gear first
        IsStarting(); // Check if the car should start
        IsDriving();  // Check if the car should continue driving
        HandleMotor(); // Apply motor force based on inputs
        HandleSteering(); // Handle steering
        UpdateWheels(); // Update visual wheel positions
        CalculateSpeed(); // Calculate and log speed
        Debug.Log("Speed: " + speed + " km/h " + gear);
        //if(gear==0)Debug.Log(isGear);
        //else Debug.Log(gear);

    }

    private void Update()
    {
        if (isGear) // Only allow gear change when clutch is pressed
        {
            if (gear1Action.triggered) gear = 1;
            else if (gear2Action.triggered) gear = 2;
            else if (gear3Action.triggered) gear = 3;
            else if (gear4Action.triggered) gear = 4;
            else if (gear5Action.triggered) gear = 5;
            else if (gearRAction.triggered) gear = -1;
        }
    }


    private void CalculateSpeed()
    {
        speed = carRigidbody.linearVelocity.magnitude * 3.6f; // Convert m/s to km/h
    }

    private void ConfigureFriction(WheelCollider wheelCollider)
    {
        WheelFrictionCurve forwardFriction = wheelCollider.forwardFriction;
        forwardFriction.stiffness = 1.5f; // Adjust as needed
        wheelCollider.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
        sidewaysFriction.stiffness = 2.0f; // Adjust as needed
        wheelCollider.sidewaysFriction = sidewaysFriction;
    }

    private void IsStarting()
    {
        // Start the car when accelerator is pressed, even if the clutch is not pressed
        if (verticalInput != 0 && carRigidbody.linearVelocity.magnitude <= 0.1f)
        {
            isStart = true;
            isDrive = true;
            Debug.Log("Car started moving.");
        }
    }



    private void IsDriving()
    {
        if (isStart && Mathf.Abs(verticalInput) > 0)
        {
            isDrive = true; // Keep driving as long as there's input
        }
        else if (speed < 0.1f && Mathf.Abs(verticalInput) == 0)
        {
            isDrive = false; // Only stop driving when completely stopped and no input
            //Debug.Log("Car stopped.");
        }
    }

    private void UpdateGear()
    {
        if (isGear) // Clutch must be pressed to change gear
        {
            if (gear == 1)
            {
                motorForce = 300;
                //Debug.Log("Gear 1 engaged");
            }
            else if (gear == 2 && speed >= 10f && speed < 40f)
            {
                motorForce = 500;
                //Debug.Log("Gear 2 engaged");
            }
            else if (gear == 3 && speed >= 40f && speed < 60f)
            {
                motorForce = 700;
                //Debug.Log("Gear 3 engaged");
            }
            else if (gear == 4 && speed >= 60f && speed < 80f)
            {
                motorForce = 1000;
                //Debug.Log("Gear 4 engaged");
            }
            else if (gear == 5 && speed >= 80f)
            {
                motorForce = 15500;
                //Debug.Log("Gear 5 engaged");
            }
            else if (gear == -1 && speed <= 5f)
            {
                motorForce = -300;
                //Debug.Log("Reverse gear engaged");
            }
            else
            {
                gear = 0; // Gear is 0 when no gear is engaged
                motorForce = 0; // No motor force when in neutral
            }
        }
    }



    private void HandleMotor()
    {
        if (isStart && isDrive) // The car is already started
        {
            float acceleratorInput = Input.GetAxis("Accelerator"); // Read accelerator input

            if (acceleratorInput > 0.1f) // Apply force if accelerator is pressed
            {
                float appliedMotorForce = acceleratorInput * motorForce * acceleratorSensitivity; // Apply sensitivity
                frontLeftWheelColider.motorTorque = appliedMotorForce;
                frontRightWheelColider.motorTorque = appliedMotorForce;
            }
            else
            {
                frontLeftWheelColider.motorTorque = 0f;
                frontRightWheelColider.motorTorque = 0f;
            }
        }
        else
        {
            frontLeftWheelColider.motorTorque = 0f;
            frontRightWheelColider.motorTorque = 0f;
        }

        // Update braking force
        cureenrBreakForce = isBreaking ? breakForce : 0f;

        if (isBreaking)
        {
            ApplyBreaking();
        }
        else
        {
            ReleaseBrakes();
        }
    }





    private void ApplyBreaking()
    {
        frontLeftWheelColider.brakeTorque = cureenrBreakForce;
        frontRightWheelColider.brakeTorque = cureenrBreakForce;
        backLeftWheelColider.brakeTorque = cureenrBreakForce;
        backRightWheelColider.brakeTorque = cureenrBreakForce;
    }

    private void ReleaseBrakes()
    {
        frontLeftWheelColider.brakeTorque = 0f;
        frontRightWheelColider.brakeTorque = 0f;
        backLeftWheelColider.brakeTorque = 0f;
        backRightWheelColider.brakeTorque = 0f;
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);

        // Get pedal values
        float clutchInput = Input.GetAxis("Clutch");  // Clutch (Z)
        float brakeInput = Input.GetAxis("Brake");    // Brake (Rz)
        float acceleratorInput = Input.GetAxis("Accelerator"); // Accelerator (HatSwitch/Y)

        // Debugging
        Debug.Log($"Clutch: {clutchInput}, Brake: {brakeInput}, Accelerator: {acceleratorInput}");

        // Clutch Logic (Must be pressed to shift gears)
        if (clutchInput > 0.8) isGear = true;
        else isGear = false;

        // Braking logic
        isBreaking = brakeInput > 0.1f;

        // Update the car start condition based on accelerator pedal input
        if (acceleratorInput > 0.1f && isGear) // Only allow start if clutch is engaged
        {
            isStart = true;
            isDrive = true;
        }
    }



    private void HandleSteering()
    {
        float targetSteerAngle = maxSteerAngle * horizontalInput;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.fixedDeltaTime * 5f);
        frontLeftWheelColider.steerAngle = currentSteerAngle;
        frontRightWheelColider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelColider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelColider, frontRightWheelTransform);
        UpdateSingleWheel(backLeftWheelColider, backLeftWheelTransform);
        UpdateSingleWheel(backRightWheelColider, backRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelColider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelColider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    public int Gear
    {
        get { return gear; } // Gear is the private field in your CarController
    }

}