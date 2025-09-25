using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTest : MonoBehaviour
{
    public GameObject Target;

    void Start()
    {
        CameraMovement.TargetTracking(Target.transform, Vector3.zero);
    }
}
