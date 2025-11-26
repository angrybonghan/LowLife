using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CrosshairController : MonoBehaviour
{
    [Header("조준점 설정")]
    public bool hideAtStart = true;

    [HideInInspector] public float currentZ = 0f;

    private Camera mainCamera;
    private SpriteRenderer SR;

    public static CrosshairController instance { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        SR = GetComponent<SpriteRenderer>();
    }

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

        ToggleSprite(!hideAtStart);
    }

    private void LateUpdate()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = -mainCamera.transform.position.z + currentZ;
        Vector3 finalCrosshairPos = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        finalCrosshairPos.z = currentZ;
        transform.position = finalCrosshairPos;
    }

    public void ToggleSprite(bool isVisible)
    {
        SR.enabled = isVisible;
    }
}