using UnityEngine;

public class ExclamationMarkTest : MonoBehaviour
{
    public float maxChargeTime = 1f;
    public bool isTest = false;

    private ExclamationMarkHandler testScrip;
    private ExclamationMarkHandler_Old testScrip_old;
    private float value;



    private void Awake()
    {
        testScrip = GetComponent<ExclamationMarkHandler>();
        testScrip_old = GetComponent<ExclamationMarkHandler_Old>();
        value = 0;
    }

    private void Start()
    {
        SetGaugeValue(value);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            value += Time.deltaTime / maxChargeTime;
            value = Mathf.Clamp01(value);
            SetGaugeValue(value);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            value -= Time.deltaTime / maxChargeTime;
            value = Mathf.Clamp01(value);
            SetGaugeValue(value);
        }
    }

    void SetGaugeValue(float value)
    {
        if (isTest)
        {
            testScrip_old.SetGaugeValue(value);
        }
        else
        {
            testScrip.SetGaugeValue(value);
        }
    }
}
