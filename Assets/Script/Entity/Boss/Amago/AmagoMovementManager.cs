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
    public AmagoBodyMovement bodyPrefab;
    public float bodySpawnDistanceInterval = 0.4f;
    public int maxBodyCount = 15;

    [Header("머리")]
    public AmagoHeadMovement headPrefab;


    Transform lastSpawnPartPos;

    int currentBodyCount = 0;
    int layerNumber = -10;
    bool canSpawnAmago = false;
    bool canSpawnBody = true;
    List<AmagoBodyMovement> allBody = new List<AmagoBodyMovement>();
    AmagoHeadMovement head;

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
    }

    void Update()
    {
        if (!canSpawnAmago) return;
        if (canSpawnBody) BodySpawnHandler();
        UpdateSpeed();
    }

    void BodySpawnHandler()
    {
        if (ShouldSpawnBody())
        {
            SummonBody();
        }
    }

    public void StartSpawnAmago()
    {
        canSpawnAmago = true;

        head = Instantiate(headPrefab, transform.position, Quaternion.identity);

        head.transform.position = transform.position;
        head.currentRoadTarget = firstRoad;
        head.rotationDuration = rotationDuration;
        head.transform.SetParent(transform);
        lastSpawnPartPos = head.transform;
    }

    void SummonBody()
    {
        AmagoBodyMovement newBody = Instantiate(bodyPrefab, transform.position, Quaternion.identity);
        newBody.currentRoadTarget = firstRoad;
        newBody.SetOrderInLayer(layerNumber);
        newBody.rotationDuration = rotationDuration;
        newBody.transform.SetParent(transform);
        allBody.Add(newBody);
        lastSpawnPartPos = newBody.transform;

        layerNumber--;
        currentBodyCount++;

        if (currentBodyCount >= maxBodyCount)
        {
            canSpawnBody = false;
            newBody.lastBody = true;
        }
    }

    void UpdateSpeed()
    {
        if (head==null)
        {
            SetAmagoSpeed(maxMoveSpeed);
            return;
        }

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
