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

	public int gear = 0;
	private bool isGear = false; // Gear pedal state
	private bool isStart = false;
	private bool isDrive = false;

	[SerializeField] private Rigidbody carRigidbody; // Assign this in the Inspector
	public float speed;

	[SerializeField] public float motorForce = 0; // This will change with gears
	[SerializeField] private float breakForce;
	[SerializeField] private float maxSteerAngle;

	[SerializeField] public WheelCollider frontLeftWheelColider;
	[SerializeField] public WheelCollider frontRightWheelColider;
	[SerializeField] public WheelCollider backLeftWheelColider;
	[SerializeField] public WheelCollider backRightWheelColider;

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

	public float Speed { get; private set; }
	private Rigidbody rb;

	public bool isSteeringWheelConnected = false;

	[SerializeField] private float[] gearSpeedLimits = new float[] { 0f, 20f, 40f, 60f, 80f, 100f }; // Speed limits for each gear (0: Neutral, 1-5: Gears, -1: Reverse)

	[SerializeField] private float slopeForce = 100f;
	public bool isManualBrake = false; // Manual brake state

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

		gear1Action = gearShifterMap.FindAction("gear1");
		gear2Action = gearShifterMap.FindAction("gear2");
		gear3Action = gearShifterMap.FindAction("gear3");
		gear4Action = gearShifterMap.FindAction("gear4");
		gear5Action = gearShifterMap.FindAction("gear5");
		gearRAction = gearShifterMap.FindAction("gearR");

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
		UpdateGear(); // Update gear first
		IsStarting(); // Check if the car should start
		IsDriving();  // Check if the car should continue driving
		HandleMotor(); // Apply motor force based on inputs
		HandleSteering(); // Handle steering
		UpdateWheels(); // Update visual wheel positions
		CalculateSpeed(); // Calculate and log speed
		CalculateRPM(); // Calculate RPM based on speed and gear
		HandleSlope(); // Handle slope forces
		Debug.Log("Speed: " + speed + " km/h, Gear: " + gear + ", RPM: " + currentRPM + ", ManualBreak: " + isManualBrake);
	}

	private void Update()
	{
		if (isGear) // Only allow gear change when clutch is pressed
		{
			if (gear1Action.triggered || Input.GetKeyDown(KeyCode.Alpha1)) gear = 1;
			else if (gear2Action.triggered || Input.GetKeyDown(KeyCode.Alpha2)) gear = 2;
			else if (gear3Action.triggered || Input.GetKeyDown(KeyCode.Alpha3)) gear = 3;
			else if (gear4Action.triggered || Input.GetKeyDown(KeyCode.Alpha4)) gear = 4;
			else if (gear5Action.triggered || Input.GetKeyDown(KeyCode.Alpha5)) gear = 5;
			else if (gearRAction.triggered || Input.GetKeyDown(KeyCode.R)) gear = -1;
		}

		// Manual brake input
		if (Input.GetKeyDown(KeyCode.B))
		{
			isManualBrake = !isManualBrake;
		}

		Speed = rb.linearVelocity.magnitude;

		if (isManualBrake)
		{
			ApplyBreaking();
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
			if (gear == 1 && speed <= gearSpeedLimits[1])
			{
				motorForce = 5000;
			}
			else if (gear == 2 && speed >= 10f && speed <= gearSpeedLimits[2])
			{
				motorForce = 1700;
			}
			else if (gear == 3 && speed >= 40f && speed <= gearSpeedLimits[3])
			{
				motorForce = 11000;
			}
			else if (gear == 4 && speed >= 60f && speed <= gearSpeedLimits[4])
			{
				motorForce = 115500;
			}
			else if (gear == 5 && speed >= 80f && speed <= gearSpeedLimits[5])
			{
				motorForce = 11550000;
			}
			else if (gear == -1 && speed <= 5f)
			{
				motorForce = -1500;
			}
			/*else
			{
				gear = 0; // Gear is 0 when no gear is engaged
				motorForce = 0; // No motor force when in neutral
			}

			// Enforce speed limits
			if (gear > 0 && gear < gearSpeedLimits.Length && speed > gearSpeedLimits[gear])
			{
				motorForce = 0; // Stop applying force if speed exceeds the limit
			}
			*/
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
		if (isStart && isDrive && gear != 0)
		{
			// Calculate base motor force
			float appliedMotorForce = verticalInput * motorForce * acceleratorSensitivity;

			// Detect if we're on a slope
			bool onSlope = IsOnSlope(out float slopeAngle);

			// Add extra torque when starting on a slope
			if (onSlope && speed < 5f && verticalInput > 0.1f)
			{
				float slopeBoost = Mathf.Clamp(slopeAngle / 10f, 1f, 3f); // 1x-3x boost based on slope
				appliedMotorForce *= slopeBoost;
			}

			// Release brakes when accelerating
			if (verticalInput > 0.1f && !isManualBrake)
			{
				ReleaseBrakes();
			}

			// Apply force to wheels
			frontLeftWheelColider.motorTorque = appliedMotorForce;
			frontRightWheelColider.motorTorque = appliedMotorForce;
		}
		else
		{
			frontLeftWheelColider.motorTorque = 0f;
			frontRightWheelColider.motorTorque = 0f;
		}
	}


	public void ApplyBreaking()
	{
		// Only apply full brakes if not accelerating
		if (Mathf.Abs(verticalInput) < 0.1f || isManualBrake)
		{
			float effectiveBrakeForce = isManualBrake ?
				breakForce * 10f :
				breakForce;

			frontLeftWheelColider.brakeTorque = effectiveBrakeForce;
			frontRightWheelColider.brakeTorque = effectiveBrakeForce;
			backLeftWheelColider.brakeTorque = effectiveBrakeForce;
			backRightWheelColider.brakeTorque = effectiveBrakeForce;
		}
		else
		{
			// Light braking when accelerating on slopes to prevent rollback
			if (IsOnSlope(out float angle) && angle > 10f)
			{
				float slopeBrake = Mathf.Clamp(angle * 10f, 0f, breakForce / 2f);
				backLeftWheelColider.brakeTorque = slopeBrake;
				backRightWheelColider.brakeTorque = slopeBrake;
			}
		}
	}

	public void ReleaseBrakes()
	{
		frontLeftWheelColider.brakeTorque = 0f;
		frontRightWheelColider.brakeTorque = 0f;
		backLeftWheelColider.brakeTorque = 0f;
		backRightWheelColider.brakeTorque = 0f;
	}

	private void HandleSlopeStart()
	{
		if (IsOnSlope(out float slopeAngle) &&
			gear != 0 &&
			verticalInput > 0.1f &&
			speed < 2f)
		{
			// Temporary torque boost for 1 second
			float boostFactor = Mathf.Clamp(slopeAngle / 15f, 1f, 3f);
			frontLeftWheelColider.motorTorque *= boostFactor;
			frontRightWheelColider.motorTorque *= boostFactor;

			// Reduce rear brakes slightly
			backLeftWheelColider.brakeTorque *= 0.7f;
			backRightWheelColider.brakeTorque *= 0.7f;
		}
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

	private void HandleSlope()
	{
		// Detect if we're on a slope
		if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 2f))
		{
			float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

			// If manual brake is on, apply maximum braking and return early
			if (isManualBrake)
			{
				ApplyBreaking();
				return; // Skip all other slope handling when manual brake is on
			}

			// Only apply slope forces if angle is significant and manual brake is off
			if (slopeAngle > 5f && !isManualBrake)
			{
				// Calculate slope force direction (down the slope)
				Vector3 slopeForceDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;

				// Apply anti-rollback force when stationary or rolling backward
				if ((gear == 1 || gear == -1) &&
					(carRigidbody.linearVelocity.magnitude < 0.5f ||
					 Vector3.Dot(carRigidbody.linearVelocity, transform.forward) < 0))
				{
					float antiRollForce = slopeAngle * carRigidbody.mass * 0.1f;
					carRigidbody.AddForce(-slopeForceDirection * antiRollForce, ForceMode.Force);
				}

				// Apply natural gravity force down the slope
				float gravityForce = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * carRigidbody.mass * 9.81f;
				carRigidbody.AddForce(slopeForceDirection * gravityForce, ForceMode.Force);
			}
		}

		// Apply brakes if needed
		if (isManualBrake || isBreaking)
		{
			ApplyBreaking();
		}
		else
		{
			ReleaseBrakes();
		}
	}

	private bool IsOnSlope(out float slopeAngle)
	{
		if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, 2f))
		{
			slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
			return slopeAngle > 5f; // Only consider it a slope if >5 degrees
		}
		slopeAngle = 0f;
		return false;
	}
}
