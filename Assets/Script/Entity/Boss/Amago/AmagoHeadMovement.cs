using System.Collections;
using UnityEngine;

public class AmagoHeadMovement : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public AmagoRoadSelector currentRoadTarget;
    [HideInInspector] public float rotationDuration;

    Vector2 currentDirection;
    Coroutine rotateCoroutine;


    void Update()
    {
        GoFowordToRoad();
        SetNextRoad();
    }

    void GoFowordToRoad()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            currentRoadTarget.transform.position,
            speed * Time.deltaTime
            );
    }

    void SetNextRoad()
    {
        if ((Vector2)transform.position != (Vector2)currentRoadTarget.transform.position) return;

        currentRoadTarget.SetConfirmedRoad();
        currentRoadTarget = currentRoadTarget.GetNextRoad();

        if (currentRoadTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        currentDirection = (Vector2)(currentRoadTarget.transform.position - transform.position);
        currentDirection = currentDirection.normalized;

        LookNextRoad();
    }

    void LookNextRoad()
    {
        if (currentDirection.sqrMagnitude < 0.001f) return;

        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }

        rotateCoroutine = StartCoroutine(RotateToNextRoadCoroutine());
    }
    
    private IEnumerator RotateToNextRoadCoroutine()
    {
        float targetAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        if (rotationDuration > 0)
        {
            Quaternion startRotation = transform.rotation;
            float timeElapsed = 0f;

            while (timeElapsed < rotationDuration)
            {
                float t = timeElapsed / rotationDuration;
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
                timeElapsed += Time.deltaTime;
            }
        }

        transform.rotation = targetRotation;
        rotateCoroutine = null;
    }

}
