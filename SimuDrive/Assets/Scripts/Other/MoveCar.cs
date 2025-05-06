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
		if (!Overtake.carOutsideTheRoad && (/*overtake do it the same*/ RoadJunctionSecondRoad.isCarStart))
			rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
	}


}
