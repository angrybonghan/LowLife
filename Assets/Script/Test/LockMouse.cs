//using UnityEngine;

//public class LockMouse : MonoBehaviour
//{
//    [Header("¼³Á¤")]
//    public bool lockMouseAtStart;

//    private bool isMouseLocked = false;

//    private void Start()
//    {
//        if (lockMouseAtStart)
//        {
//            LockmodeSwitch();
//        }
//    }

//    private void LockmodeSwitch()
//    {
//        isMouseLocked = !isMouseLocked;

//        if (isMouseLocked)
//        {
//            Cursor.visible = false;
//            Cursor.lockState = CursorLockMode.Confined;
//        }
//        else
//        {
//            Cursor.visible = true;
//            Cursor.lockState = CursorLockMode.None;
//        }
//    }

//    public void UnlockMouseForUI()
//    {
//        Cursor.visible = true;
//        Cursor.lockState = CursorLockMode.None;
//    }
//}