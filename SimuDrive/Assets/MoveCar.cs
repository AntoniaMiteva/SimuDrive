using UnityEngine;

public class MoveCar : MonoBehaviour
{
	public float speed = 10f;
	private Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		if (!Overtake.carOutsideTheRoad)
			rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
	}


}
