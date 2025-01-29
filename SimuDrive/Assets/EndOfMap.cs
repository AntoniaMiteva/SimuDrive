using UnityEngine;

public class EndOfMap : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("You reached the end of the map!");
            other.transform.position = new Vector3(0, 1, 0); // Teleport back to start
        }
    }
}
