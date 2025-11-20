using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("조준점 설정")]
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
            Debug.LogError("씬에 'MainCamera' 태그가 지정된 카메라가 없음");
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