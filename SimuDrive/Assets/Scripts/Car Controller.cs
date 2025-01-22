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
        if (isGear && !isDrive && (gear == 1 || gear == -1) && carRigidbody.linearVelocity.magnitude <= 0.1f &&
            (verticalInput != 0 || horizontalInput != 0))
        {
            isStart = true;
            isDrive = true;
            Debug.Log("Car started moving.");
        }
    }




    private void IsDriving()
    {
        if (isStart && (Mathf.Abs(verticalInput) > 0 || Mathf.Abs(horizontalInput) > 0))
        {
            isDrive = true; // Keep driving
        }
        else if (Mathf.Abs(verticalInput) == 0 && Mathf.Abs(horizontalInput) == 0)
        {
            isDrive = false; // Stop driving if no input
            isStart = false; // Require Shift to restart after stopping
            Debug.Log("Car stopped.");
        }
    }




    private void UpdateGear()
    {
        isGear = Input.GetKey(KeyCode.LeftShift);
        if (isGear && Input.GetKey(KeyCode.Alpha1))
        {
            gear = 1;
            motorForce = 80;
            Debug.Log("gear 1");
        }
        else if (isGear && Input.GetKey(KeyCode.Alpha2))
        {
            gear = 2;
            motorForce = 200;
            Debug.Log("gear 2");
        }
        else if (isGear && Input.GetKey(KeyCode.Alpha3))
        {
            gear = 3;
            motorForce = 400;
            Debug.Log("gear 3");
        }
        else if (isGear && Input.GetKey(KeyCode.Alpha4))
        {
            gear = 4;
            motorForce = 600;
            Debug.Log("gear 4");
        }
        else if (isGear && Input.GetKey(KeyCode.Alpha5))
        {
            gear = 5;
            motorForce = 800;
            Debug.Log("gear 5");
        }
        else if (isGear && Input.GetKey(KeyCode.R))
        {
            gear = -1;
            motorForce = -80;
            Debug.Log("Reverse gear");
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            timer = Time.time;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Time.time - timer > holdDur)
            {
                timer -= float.PositiveInfinity;
            }
        }
        else
        {
            timer = float.PositiveInfinity;
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