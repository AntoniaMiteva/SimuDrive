using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private bool isGear = false;
    private bool isStart = false;
    private bool isDrive = false;

    [SerializeField] private Rigidbody carRigidbody; // Assign this in the Inspector
    private float speed;


    [SerializeField] private float motorForce = 0; //shte se promenq ot gears
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
        // Require Shift to start moving if the car is stationary (speed <= 0)
        if (Input.GetKey(KeyCode.LeftShift) && !isDrive && (gear == 1 || gear == -1) && carRigidbody.linearVelocity.magnitude <= 0.1f &&
            (verticalInput != 0 || horizontalInput != 0))
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
            Debug.Log("Car stopped.");
        }
    }

    private void CalculateRPM()
    {
        float speedFactor = speed / 100f; // Normalize speed
        currentRPM = Mathf.Lerp(minRPM, maxRPM, speedFactor);

        // Prevent RPM from exceeding limits
        currentRPM = Mathf.Clamp(currentRPM, minRPM, maxRPM);
    }



    private void UpdateGear()
    {
        // Only allow gear changes if Shift is pressed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && speed < 20f)
            {
                gear = 1;
                motorForce = 100;
                Debug.Log("Gear 1");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && speed >= 10f && speed < 40f)
            {
                gear = 2;
                motorForce = 250;
                Debug.Log("Gear 2");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && speed >= 40f && speed < 60f)
            {
                gear = 3;
                motorForce = 500;
                Debug.Log("Gear 3");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) && speed >= 60f && speed < 80f)
            {
                gear = 4;
                motorForce = 700;
                Debug.Log("Gear 4");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5) && speed >= 80f)
            {
                gear = 5;
                motorForce = 1000;
                Debug.Log("Gear 5");
            }
            else if (Input.GetKeyDown(KeyCode.R) && speed <= 5f)
            {
                gear = -1;
                motorForce = -100;
                Debug.Log("Reverse gear");
            }
        }
    }



    private void HandleMotor()
    {
        if (isStart && isDrive)
        {
            frontLeftWheelColider.motorTorque = verticalInput * motorForce;
            frontRightWheelColider.motorTorque = verticalInput * motorForce;
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
        isBreaking = Input.GetKey(KeyCode.Space);
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
}