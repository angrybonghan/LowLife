using UnityEngine;

public class HoneycombCorridorArrow : MonoBehaviour
{
    Transform target;
    Vector2 pos;

    private void Start()
    {
        target = PlayerController.instance.transform;
        if (target == null)
        {
            Destroy(gameObject);
            this.enabled = false;
        }

        pos = transform.position;
    }

    void Update()
    {
        pos.x = target.position.x;
        transform.position = pos;
    }
}
