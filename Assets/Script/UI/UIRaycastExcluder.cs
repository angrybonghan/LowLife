using UnityEngine;
using UnityEngine.UI;

public class UIRaycastExcluder : MonoBehaviour
{
    void Start()
    {
        Graphic go = GetComponent<Graphic>();
        if (go != null) go.raycastTarget = false;
        this.enabled = false;
    }
}
