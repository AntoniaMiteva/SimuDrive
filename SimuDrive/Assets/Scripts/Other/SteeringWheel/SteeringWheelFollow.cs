using UnityEngine;

public class SteeringWheelFollow : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [Range(-180f, 0f)]
    [SerializeField] private float minRotationAngle = -45f;
    [Range(0f, 180f)]
    [SerializeField] private float maxRotationAngle = 45f;

    
    private RectTransform rectTransform;
    private Transform wheelTransform;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        
        Vector3 mousePosition = Input.mousePosition;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            mousePosition,
            null,
            out localPoint);

        Vector2 direction = localPoint - (Vector2)rectTransform.localPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        RotateWheel(angle, rectTransform);
        
    }

    private void RotateWheel(float targetAngle, Transform transformToRotate)
    {
        while (targetAngle > 180f) targetAngle -= 360f;
        while (targetAngle < -180f) targetAngle += 360f;

        targetAngle = Mathf.Clamp(targetAngle, minRotationAngle, maxRotationAngle);

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transformToRotate.rotation = Quaternion.Slerp(
                transformToRotate.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
    }
}
