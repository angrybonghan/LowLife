using TMPro;
using UnityEngine;

public class CameraMovement_Test : MonoBehaviour
{
    public Transform targetPosition;
    private Vector3 mainPosition;

    private float cameraTrackingSpeed = 8;
    private float yTrackingDampening = 2;

    private Vector3 velocity = Vector3.zero; // 현재 속도. 시간이 지남에 따라 변함
    private float smoothTime = 0.2f;

    void Start()
    {
        
    }

    private void LateUpdate()
    {
        Vector3 targetZPosition = new Vector3(targetPosition.position.x, targetPosition.position.y, transform.position.z);

        // SmoothDamp는 현재 위치에서 목표 위치로 부드럽게 이동시킵니다.
        // smoothTime과 함께 마지막 인수로 최대 속도 제한을 줄 수 있습니다.
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetZPosition,
            ref velocity,
            smoothTime
        );
    }
}
