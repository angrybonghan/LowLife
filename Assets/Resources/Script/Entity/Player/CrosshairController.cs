using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("조준점 설정")]
    public float mouseSensitivity = 10f;
    public float maxMovementRange = 5f;
    public float currentZ = 0;

    private Transform cameraPosition;
    private Vector2 finalCameraPosition;

    void Start()
    {
        GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
        this.transform.SetParent(mainCameraObject.transform);

        cameraPosition = mainCameraObject.transform;
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            finalCameraPosition = new Vector3(cameraPosition.position.x, cameraPosition.position.y, currentZ);

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            Vector3 newPosition = transform.position;
            newPosition.x += mouseX;
            newPosition.y += mouseY;

            float distance = Vector3.Distance(finalCameraPosition, newPosition);

            if (distance > maxMovementRange)
            {
                Vector2 direction = (newPosition - (Vector3)finalCameraPosition).normalized;
                newPosition = finalCameraPosition + direction * maxMovementRange;
            }

            newPosition.z = currentZ;

            transform.position = newPosition;
        }
    }
}
