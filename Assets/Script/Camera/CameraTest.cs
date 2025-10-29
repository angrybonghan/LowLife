using UnityEngine;

public class CameraTest : MonoBehaviour
{
    public GameObject Target;

    void Start()
    {
        CameraMovement.TargetTracking(Target.transform, new Vector3(0,1.5f,0));
        //CameraMovement.RotateTo(new Vector3(7,0,0), 0f);
    }
}
