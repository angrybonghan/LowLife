using UnityEngine;

public class NextStage : MonoBehaviour
{
    public string nextStageName;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.RightAlt)&&Input.GetKey(KeyCode.RightShift))
        {
            Debug.Log("Next Stage Load: " + nextStageName);
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextStageName);
        }
    }
}
