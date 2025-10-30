using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("������ ����")]
    public float currentZ = 0f;

    private Camera mainCamera;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCamera = Camera.main;
        }
        else
        {
            Debug.LogError("���� 'MainCamera' �±װ� ������ ī�޶� ����");
            enabled = false;
        }
    }

    private void Update()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = -mainCamera.transform.position.z + currentZ;
        Vector3 finalCrosshairPos = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        finalCrosshairPos.z = currentZ;
        transform.position = finalCrosshairPos;
    }
}