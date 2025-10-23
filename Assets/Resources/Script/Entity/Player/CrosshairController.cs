using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("조준점 설정")]
    public float mouseSensitivity = 10f;
    public float maxMovementRange = 5f;
    public float currentZ = 0;

    private Transform anchorPosition;
    private Vector3 offset;

    void Start()
    {
        GameObject anchorObject = GameObject.FindWithTag("Player");
        anchorPosition = anchorObject.transform;
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector3 currentAnchorPosition = anchorPosition.position;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        Vector3 newoffset = offset;
        newoffset.x += mouseX;
        newoffset.y += mouseY;

        Vector3 finalCrosshairPos = currentAnchorPosition + newoffset;

        float distance = Vector3.Distance(currentAnchorPosition, finalCrosshairPos);

        if (distance > maxMovementRange)
        {
            Vector3 direction = (finalCrosshairPos - currentAnchorPosition).normalized;
            finalCrosshairPos = currentAnchorPosition + direction * maxMovementRange;
        }

        finalCrosshairPos.z = currentZ;
        offset = finalCrosshairPos - currentAnchorPosition;

        transform.position = finalCrosshairPos;
    }
}