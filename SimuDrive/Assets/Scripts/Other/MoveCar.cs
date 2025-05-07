using Unity.VisualScripting;
using UnityEngine;

public class MoveCar : MonoBehaviour
{
	public float speed = 10f;
	private Rigidbody rb;
	[SerializeField] private CarController carController;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		if (!Overtake.carOutsideTheRoad && (Overtake.isStart || Overtake2.isStart || RoadJunctionSecondRoad.isCarStart || RoadJunctionNoControl.carLevelStart))
			rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
	}

	public int carCubeCheck = 0;

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("AnotherCarCube"))
		{
			carCubeCheck++;
		}
	}
}
