using UnityEngine;

public class Speedometers : MonoBehaviour
{
	public Transform speedArrow; // Renamed to avoid ambiguity
	public float maxSpeedLimit = 120f; // Renamed to avoid ambiguity
	public float minAngle = 137.5f; // Minimum angle of the arrow (pointing to 0 speed)
	public float maxAngle = -137.5f; // Maximum angle of the arrow (pointing to max speed)
	[SerializeField] private CarController carController; // Reference to CarController
	private float currentSpeed; // Renamed to avoid ambiguity

	void Update()
	{
		// Get the speed from the car controller dynamically
		if (carController != null)
		{
			currentSpeed = carController.speed; // Assuming `speed` is a public field in CarController
		}

		// Map the currentSpeed to the angle for the arrow
		float angle = Mathf.Lerp(minAngle, maxAngle, currentSpeed / maxSpeedLimit);

		// Rotate the arrow to the correct angle
		speedArrow.localRotation = Quaternion.Euler(0, 0, angle);
	}
}
