using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float delayTime = 3f;
    void Start()
    {
        Destroy(gameObject, delayTime);
    }
}
