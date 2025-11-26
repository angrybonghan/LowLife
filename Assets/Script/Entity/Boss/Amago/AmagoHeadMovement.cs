using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AmagoHeadMovement : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public AmagoRoadSelector currentRoadTarget;


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

        Vector3 direction = currentRoadTarget.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void SetNextRoad()
    {
        if ((Vector2)transform.position != (Vector2)currentRoadTarget.transform.position) return;

        currentRoadTarget.SetConfirmedRoad();
        currentRoadTarget = currentRoadTarget.GetNextRoad();
    }
    
}
