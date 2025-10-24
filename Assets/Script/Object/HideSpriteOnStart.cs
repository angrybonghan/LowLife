using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HideSpriteOnStart : MonoBehaviour
{
    void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        this.enabled = false;
    }
}