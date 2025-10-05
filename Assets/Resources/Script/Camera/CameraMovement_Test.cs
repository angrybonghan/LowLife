using TMPro;
using UnityEngine;

public class CameraMovement_Test : MonoBehaviour
{
    public Transform targetPosition;
    private Vector3 mainPosition;

    private float cameraTrackingSpeed = 8;
    private float yTrackingDampening = 2;

    private Vector3 velocity = Vector3.zero; // ���� �ӵ�. �ð��� ������ ���� ����
    private float smoothTime = 0.2f;

    void Start()
    {
        
    }

    private void LateUpdate()
    {
        Vector3 targetZPosition = new Vector3(targetPosition.position.x, targetPosition.position.y, transform.position.z);

        // SmoothDamp�� ���� ��ġ���� ��ǥ ��ġ�� �ε巴�� �̵���ŵ�ϴ�.
        // smoothTime�� �Բ� ������ �μ��� �ִ� �ӵ� ������ �� �� �ֽ��ϴ�.
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetZPosition,
            ref velocity,
            smoothTime
        );
    }
}
