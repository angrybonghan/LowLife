using TMPro;
using UnityEngine;

public class RandomTmpText : MonoBehaviour
{
    public string[] randomTexts;

    private TextMeshPro TMPtext;

    private void Awake()
    {
        TMPtext = GetComponent<TextMeshPro>();
    }

    void Start()
    {
        int randomNumber = Random.Range(0, randomTexts.Length - 1);
        string selectedText = randomTexts[randomNumber];

        TMPtext.text = selectedText;
    }
}
