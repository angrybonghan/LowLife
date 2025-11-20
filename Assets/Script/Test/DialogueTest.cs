using UnityEngine;

public class DialogueTest : MonoBehaviour
{
    public DialogueSO someTuffDialogue;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            DialogManager.instance.StartDialogue(someTuffDialogue, gameObject);
        }
    }
}
