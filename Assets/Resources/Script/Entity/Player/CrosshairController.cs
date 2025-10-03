using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("조준점 설정")]
    public float mouseSensitivity = 10f;
    public float maxMovementRange = 5f;
    public float ZposcurrentZ = 0;

    private Transform cameraPosition;

    void Start()
    {
        GameObject mainCameraObject = GameObject.FindWithTag("MainCamera");
        this.transform.SetParent(mainCameraObject.transform);

        cameraPosition = mainCameraObject.transform;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        Vector3 newPosition = transform.position;
        newPosition.x += mouseX;
        newPosition.y += mouseY;

        float distance = Vector3.Distance(cameraPosition.position, newPosition);

        if (distance > maxMovementRange)
        {
            Vector3 direction = (newPosition - cameraPosition.position).normalized;
            newPosition = cameraPosition.position + direction * maxMovementRange;
        }

        transform.position = newPosition;
    }
}
