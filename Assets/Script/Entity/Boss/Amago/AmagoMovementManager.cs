using System.Collections.Generic;
using UnityEngine;

public class AmagoMovementManager : MonoBehaviour
{
    public static AmagoMovementManager instance {  get; private set; }

    [Header("첫 번째 길")]
    public AmagoRoadSelector firstRoad;

    [Header("이동 설정")]
    public float minMoveSpeed = 5f;
    public float maxMoveSpeed = 35f;
    public float speedIncreaseSensitivity = 7f;
    public float rotationDuration = 0.2f;

    [Header("몸")]
    public AmagoBodyMovement body;
    public float bodySpawnDistanceInterval = 0.4f;
    public int maxBodyCount = 15;

    [Header("머리")]
    public AmagoHeadMovement head;

    Transform lastSpawnPartPos;

    int currentBodyCount = 0;
    int layerNumber = -2;
    bool canSpawnBody = true;
    List<AmagoBodyMovement> allBody = new List<AmagoBodyMovement>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            this.enabled = false;
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        canSpawnBody = maxBodyCount > 0;

        head.transform.position = transform.position;
        lastSpawnPartPos = head.transform;
        head.currentRoadTarget = firstRoad;
        head.rotationDuration = rotationDuration;
    }

    void Update()
    {
        if (canSpawnBody) BodySpawnHandler();
        UpdateSpeed();
    }

    void BodySpawnHandler()
    {
        if (ShouldSpawnBody())
        {
            SummonBody();
            if (currentBodyCount >= maxBodyCount) canSpawnBody = false;
        }
    }

    void SummonBody()
    {
        AmagoBodyMovement newBody = Instantiate(body, transform.position, Quaternion.identity);
        newBody.currentRoadTarget = firstRoad;
        newBody.SetOrderInLayer(layerNumber);
        newBody.rotationDuration = rotationDuration;
        allBody.Add(newBody);
        lastSpawnPartPos = newBody.transform;

        layerNumber--;
        currentBodyCount++;
    }

    void UpdateSpeed()
    {
        float distanceToPlayer = Vector2.Distance(head.transform.position, PlayerController.instance.transform.position);
        float t = Mathf.Clamp01(distanceToPlayer / speedIncreaseSensitivity);
        float speed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, t);

        SetAmagoSpeed(speed);
    }

    void SetAmagoSpeed(float value)
    {
        if (allBody.Count != 0)
        {
            foreach (AmagoBodyMovement body in allBody)
            {
                body.speed = value;
            }
        }

        head.speed = value;
    }

    bool ShouldSpawnBody()
    {
        float distanceToLastPart = Vector2.Distance(lastSpawnPartPos.position, transform.position);
        return distanceToLastPart > bodySpawnDistanceInterval;
    }
}
