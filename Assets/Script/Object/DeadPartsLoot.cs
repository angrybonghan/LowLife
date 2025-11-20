using UnityEngine;

public class DeadPartsLoot : MonoBehaviour
{
    private void Update()
    {
        if (transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetRotation(Vector3 newAngles)
    {
        transform.rotation = Quaternion.Euler(newAngles);
    }
}
