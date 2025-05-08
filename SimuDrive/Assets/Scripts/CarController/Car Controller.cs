using Microsoft.Unity.VisualStudio.Editor;
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
	private bool isGear = false; 
	public bool isStart = false;
	public bool isDrive = false;

	[SerializeField] private Rigidbody carRigidbody; 
	public float speed;

	[SerializeField] public float motorForce = 0; 
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

	[SerializeField] private float maxRPM = 7000f;
	[SerializeField] private float minRPM = 1000f;

	[SerializeField] private GameObject leftBlinkerImageOn;
	[SerializeField] private GameObject leftBlinkerImageOff;
	[SerializeField] private GameObject rightBlinkerImageOn;
	[SerializeField] private GameObject rightBlinkerImageOff;

	private float currentRPM;


	public InputActionAsset inputActions;
	private InputAction gear1Action;
	private InputAction gear2Action;
	private InputAction gear3Action;
	private InputAction gear4Action;
	private InputAction gear5Action;
	private InputAction gearRAction;

	public InputActionAsset inputAdditionalActions;
	private InputAction manualBrake;
	private InputAction leftBlinker;
	private InputAction rightBlinker;
	private InputAction emergency;
	private InputAction lookLeft;
	private InputAction lookRight;

	[SerializeField] private float acceleratorSensitivity = 2f; // Adjust this value to make the pedal more sensitive

	public float Speed { get; private set; }
	private Rigidbody rb;

	public bool isSteeringWheelConnected = false;

	[SerializeField] private float[] gearSpeedLimits = new float[] { 0f, 30f, 50f, 80f, 100f, 200f }; // Speed limits for each gear (0: Neutral, 1-5: Gears, -1: Reverse)


	public bool isManualBrake = false;
	public bool isLeftBlinker = false;
	public bool isRightBlinker = false;
	public bool isEmergency = false;
	public bool isLookLeft = false;
	public bool isLookRight = false;


	public Transform cameraTransform;


	public float Rotation
	{
		get
		{
			float yRot = transform.eulerAngles.y;
			if (yRot > 180f) yRot -= 360f;
			return yRot;
		}
	}




	private void Awake()
	{
		if (inputActions == null)
		{
			Debug.LogError("Input Actions is not assigned in the Inspector.");
			return;
		}

		var gearShifterMap = inputActions.FindActionMap("GearShifter");
		if (gearShifterMap == null)
		{
			Debug.LogError("GearShifter action map not found in Input Actions.");
			return;
		}

		var additionalButtonsMap = inputAdditionalActions.FindActionMap("AdditionalButtons");
		if (additionalButtonsMap == null)
		{
			Debug.LogError("GearShifter action map not found in Input Actions.");
			return;
		}
		

		gear1Action = gearShifterMap.FindAction("gear1");
		gear2Action = gearShifterMap.FindAction("gear2");
		gear3Action = gearShifterMap.FindAction("gear3");
		gear4Action = gearShifterMap.FindAction("gear4");
		gear5Action = gearShifterMap.FindAction("gear5");
		gearRAction = gearShifterMap.FindAction("gearR");

		manualBrake = additionalButtonsMap.FindAction("manualBrake");
		leftBlinker = additionalButtonsMap.FindAction("leftBlinker");
		rightBlinker = additionalButtonsMap.FindAction("rightBlinker");
		lookLeft = additionalButtonsMap.FindAction("lookLeft");
		lookRight = additionalButtonsMap.FindAction("lookRight");
		emergency = additionalButtonsMap.FindAction("emergencyBlinkers");

		if (gear1Action == null || gear2Action == null || gear3Action == null ||
			gear4Action == null || gear5Action == null || gearRAction == null)
		{
			Debug.LogError("One or more actions are missing in the GearShifter action map.");
			return;
		}

		gear1Action.Enable();
		gear2Action.Enable();
		gear3Action.Enable();
		gear4Action.Enable();
		gear5Action.Enable();
		gearRAction.Enable();

		manualBrake.Enable();
		leftBlinker.Enable();
		rightBlinker.Enable();
		lookLeft.Enable();
		lookRight.Enable();
		emergency.Enable();

		Debug.Log("Actions enabled successfully!");
	}

	private void Start()
	{
		carRigidbody.centerOfMass = new Vector3(0, -0.5f, 0); // Adjust Y to lower
		ConfigureFriction(frontLeftWheelColider);
		ConfigureFriction(frontRightWheelColider);
		ConfigureFriction(backLeftWheelColider);
		ConfigureFriction(backRightWheelColider);
		rb = GetComponent<Rigidbody>();
		DetectSteeringWheel();

		leftBlinkerImageOff.SetActive(true);
		leftBlinkerImageOn.SetActive(false);
		rightBlinkerImageOff.SetActive(true);
		rightBlinkerImageOn.SetActive(false);
	}
	private void DetectSteeringWheel()
	{
		foreach (var device in InputSystem.devices)
		{
			if (device.description.product != null && device.description.product.ToLower().Contains("wheel"))
			{
				isSteeringWheelConnected = true;
				Debug.Log("Steering Wheel Detected: " + device.description.product);
				return;
			}
		}
		isSteeringWheelConnected = false;
		Debug.Log("No Steering Wheel Detected. Using Keyboard Controls.");
	}
	private void FixedUpdate()
	{
		GetInput();
		IsStarting(); // Check if the car should start
		IsDriving();  // Check if the car should continue driving
		HandleMotor(); // Apply motor force based on inputs
		HandleSteering(); // Handle steering
		UpdateWheels(); // Update visual wheel positions
		CalculateSpeed(); // Calculate and log speed
		CalculateRPM(); // Calculate RPM based on speed and gear
		Debug.Log("Speed: " + speed + " km/h, Gear: " + gear + ", RPM: " + currentRPM);

	}

	private void Update()
	{
		leftBlinkerImageOff.SetActive(false);
		leftBlinkerImageOn.SetActive(false);

		if (isGear) // Only allow gear change when clutch is pressed
		{
			if (gear1Action.triggered || Input.GetKeyDown(KeyCode.Alpha1)) gear = 1;
			else if (gear2Action.triggered || Input.GetKeyDown(KeyCode.Alpha2)) gear = 2;
			else if (gear3Action.triggered || Input.GetKeyDown(KeyCode.Alpha3)) gear = 3;
			else if (gear4Action.triggered || Input.GetKeyDown(KeyCode.Alpha4)) gear = 4;
			else if (gear5Action.triggered || Input.GetKeyDown(KeyCode.Alpha5)) gear = 5;
			else if (gearRAction.triggered || Input.GetKeyDown(KeyCode.R)) gear = -1;
			else if (Input.GetKeyDown(KeyCode.Alpha0)) gear = 0;

			//Debug.Log("Gear changed to: " + gear);
		}

		Speed = rb.linearVelocity.magnitude; // Fix: Change linearVelocity to velocity



		//Manual brake input
		if (Input.GetKeyDown(KeyCode.B) || manualBrake.triggered)
		{
			isManualBrake = !isManualBrake;
		}

		Speed = rb.linearVelocity.magnitude;

		if (isManualBrake)
		{
			ApplyBreaking();
		}

		//Blinkers input
		if (Input.GetKeyDown(KeyCode.Q) || leftBlinker.triggered)
		{
			isLeftBlinker = !isLeftBlinker;
			if (isRightBlinker) isRightBlinker = false;
		}
		if (Input.GetKeyDown(KeyCode.E) || rightBlinker.triggered)
		{
			isRightBlinker = !isRightBlinker;
			if (isLeftBlinker) isLeftBlinker = false;

		}
		if (Input.GetKeyDown(KeyCode.W) || emergency.triggered)
		{
			isEmergency = !isEmergency;
		}

		if (isLeftBlinker)
		{
			leftBlinkerImageOff.SetActive(false);
			leftBlinkerImageOn.SetActive(true);
			rightBlinkerImageOff.SetActive(true);
			rightBlinkerImageOn.SetActive(false);
		}
		else if (isRightBlinker)
		{
			rightBlinkerImageOff.SetActive(false);
			rightBlinkerImageOn.SetActive(true);
			leftBlinkerImageOff.SetActive(true);
			leftBlinkerImageOn.SetActive(false);
		}
		else if(isEmergency)
		{
			leftBlinkerImageOff.SetActive(false);
			leftBlinkerImageOn.SetActive(true);
			rightBlinkerImageOff.SetActive(false);
			rightBlinkerImageOn.SetActive(true);
		}
		else
		{
			leftBlinkerImageOff.SetActive(true);
			leftBlinkerImageOn.SetActive(false);
			rightBlinkerImageOff.SetActive(true);
			rightBlinkerImageOn.SetActive(false);
		}

		//Looking around input
		if ((Input.GetKey(KeyCode.Z) || lookLeft.ReadValue<float>() > 0.5f) && cameraTransform != null)
		{
			cameraTransform.localRotation = Quaternion.Euler(3.73f, -40, 0);
		}
		else if ((Input.GetKey(KeyCode.C) || lookRight.ReadValue<float>() > 0.5f) && cameraTransform != null)
		{
			cameraTransform.localRotation = Quaternion.Euler(3.73f, 40, 0); // Example angle
		}
		else
		{
			cameraTransform.localRotation = Quaternion.Euler(3.73f, 3.17f, 0);

		}

		//Gears
		switch (gear)
		{
			case 1:
				if (speed <= gearSpeedLimits[1])
					motorForce = 3000;
				else
					motorForce = 0;
				break;

			case 2:
				if (speed >= gearSpeedLimits[1] && speed <= gearSpeedLimits[2])
					motorForce = 8000;
				else
					motorForce = 0;
				break;

			case 3:
				if (speed >= gearSpeedLimits[2] && speed <= gearSpeedLimits[3])
					motorForce = 12000;
				else
					motorForce = 0;
				break;

			case 4:
				if (speed >= gearSpeedLimits[3] && speed <= gearSpeedLimits[4])
					motorForce = 15000;
				else
					motorForce = 0;
				break;

			case 5:
				if (speed >= gearSpeedLimits[4] && speed <= gearSpeedLimits[5])
					motorForce = 300000;
				else
					motorForce = 0;
				break;

			case -1: // Reverse
				if (speed <= 5f)
					motorForce = -1500;
				else
					motorForce = 0;
				break;

			default: // Neutral or invalid
				motorForce = 0;
				break;
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

	
	private void CalculateRPM()
	{
		if (gear > 0 && gear < gearSpeedLimits.Length)
		{
			float gearRatio = speed / gearSpeedLimits[gear];
			currentRPM = Mathf.Lerp(minRPM, maxRPM, gearRatio);
		}
		else
		{
			currentRPM = minRPM; // Idle RPM when in neutral or reverse
		}
	}

	private void HandleMotor()
	{
		if (isStart && isDrive && gear != 0) // Ensure the car has started and a gear is engaged
		{
			float appliedMotorForce = verticalInput * motorForce * acceleratorSensitivity;

			// Adjust motor force based on RPM
			if (currentRPM > maxRPM * 0.9f) // Reduce force if RPM is too high
			{
				appliedMotorForce *= 0.5f;
			}
			else if (currentRPM < minRPM * 1.1f) // Reduce force if RPM is too low
			{
				appliedMotorForce *= 0.5f;
			}

			// Apply force to wheels only if accelerator is pressed
			if (verticalInput > 0.1f)
			{
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

		// Apply braking force
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






	public void ApplyBreaking()
	{
		frontLeftWheelColider.brakeTorque = cureenrBreakForce;
		frontRightWheelColider.brakeTorque = cureenrBreakForce;
		backLeftWheelColider.brakeTorque = cureenrBreakForce;
		backRightWheelColider.brakeTorque = cureenrBreakForce;
	}

	public void ReleaseBrakes()
	{
		frontLeftWheelColider.brakeTorque = 0f;
		frontRightWheelColider.brakeTorque = 0f;
		backLeftWheelColider.brakeTorque = 0f;
		backRightWheelColider.brakeTorque = 0f;
	}

	private void GetInput()
	{
		bool wheelConnected = IsSteeringWheelConnected();

		if (wheelConnected)
		{
			// Steering Wheel Input
			horizontalInput = Input.GetAxis("Steering");  // Wheel rotation
			verticalInput = Input.GetAxis("Accelerator"); // Pedal for acceleration
			isBreaking = Input.GetAxis("Brake") > 0.1f;   // Pedal for brake
			isGear = Input.GetAxis("Clutch") > 0.8f;      // Pedal for clutch
		}
		else
		{
			// Keyboard Input
			horizontalInput = Input.GetAxis("Horizontal");  // A/D or Left/Right Arrows
			verticalInput = Input.GetKey(KeyCode.UpArrow) ? 1f : 0f; // Up Arrow for acceleration
			isBreaking = Input.GetKey(KeyCode.Space);       // Space for brake
			isGear = Input.GetKey(KeyCode.LeftShift);       // Left Shift for clutch
		}

		Debug.Log("Input Mode: " + (wheelConnected ? "Steering Wheel" : "Keyboard"));
	}


	bool IsSteeringWheelConnected()
	{
		string[] joysticks = Input.GetJoystickNames();
		foreach (string joystick in joysticks)
		{
			if (!string.IsNullOrEmpty(joystick))
			{
				return true; // Steering wheel detected
			}
		}
		return false; // No wheel detected, use keyboard
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

	public void StopCar()
	{
		speed = 0f; // Set the speed to 0
					// Optionally, you can also set the car gear to neutral if needed:
		gear = 0;
		breakForce = 100000f;
	}




	//Collisions
	public bool carObstacle = false;
	public bool carAnotherCar = false;
	public bool carCubeFront = false;
	public bool carCubeBack = false;
	public bool carCubeLeft = false;
	public bool carCubeRight = false;
	public bool carBlinkChecker = false;
	public bool carBlinkChecker2 = false;
	public bool carEndOfTheRoad = false;
	public bool carStopSign = false;
	public bool carDidntStopSign = false;
	public bool carFinishLevel = false;
	public bool carParkChecker1 = false;
	public bool carParkChecker2 = false;

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("FrontCube"))
		{
			carCubeFront = true;
		}

		if (other.gameObject.CompareTag("BackCube"))
		{
			carCubeBack = true;
		}

		if (other.gameObject.CompareTag("AnotherCar"))
		{
			carAnotherCar = true;
		}

		if (other.gameObject.CompareTag("LeftCube")) 
		{
			carCubeLeft = true;
		}

		if (other.gameObject.CompareTag("RightCube"))
		{
			carCubeRight = true;
		}

		if (other.gameObject.CompareTag("BlinkChecker"))
		{
			carBlinkChecker = true;
		}

		if (other.gameObject.CompareTag("BlinkChecker2"))
		{
			carBlinkChecker2 = true;
		}

		if (other.gameObject.CompareTag("EndOfTheRoad"))
		{
			carEndOfTheRoad = true;
		}

		if (other.gameObject.CompareTag("StopSignCube"))
		{
			carStopSign = true;
		}

		if (other.gameObject.CompareTag("DidntStopCube"))
		{
			carDidntStopSign = true;
		}

		if (other.gameObject.CompareTag("CubeFinishLevel"))
		{
			carFinishLevel = true;
		}

		if (other.gameObject.CompareTag("ParkChecker1"))
		{
			carParkChecker1 = true;
		}

		if (other.gameObject.CompareTag("ParkChecker2"))
		{
			carParkChecker2 = true;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Obstacle"))
		{
			carObstacle = true;
		}
	}
}