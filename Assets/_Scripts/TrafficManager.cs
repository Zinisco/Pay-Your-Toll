using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;

    [Header("Starting Lane")]
    [SerializeField] private RoadLane startingLane;

    [Header("Additional Lanes")]
    [SerializeField] private RoadLane northboundLanePrefab;
    [SerializeField] private RoadLane southboundLanePrefab;
    [SerializeField] private float laneXOffset = 4f;
    [SerializeField] private int maximumLaneCount = 4;

    [Header("Automation")]
    [SerializeField] private bool automationUnlocked;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float minimumSpawnInterval = 0.75f;

    private readonly List<RoadLane> lanes = new();

    private Coroutine automationRoutine;

    private float currentProcessingTime = 3f;
    private int currentMoneyPerCar = 1;

    public bool AutomationUnlocked => automationUnlocked;
    public float SpawnInterval => spawnInterval;
    public float MinimumSpawnInterval => minimumSpawnInterval;

    public int LaneCount => lanes.Count;
    public int MaximumLaneCount => maximumLaneCount;

    public float ProcessingTime => currentProcessingTime;
    public int MoneyPerCar => currentMoneyPerCar;

    public float CarsPerSecond
    {
        get
        {
            if (!automationUnlocked || spawnInterval <= 0f)
                return 0f;

            return 1f / spawnInterval;
        }
    }

    private void Awake()
    {
        if (startingLane != null)
            RegisterLane(startingLane);
    }

    private void Start()
    {
        if (automationUnlocked)
            StartAutomation();
    }

    public void RegisterLane(RoadLane lane)
    {
        if (lane == null || lanes.Contains(lane))
            return;

        lane.Initialize(moneyManager);

        lanes.Add(lane);

        ApplyCurrentUpgradesToLane(lane);
    }

    public bool BuildNextLane()
    {
        if (startingLane == null)
        {
            Debug.LogError(
                "TrafficManager has no Starting Lane assigned.",
                this
            );

            return false;
        }

        if (northboundLanePrefab == null ||
            southboundLanePrefab == null)
        {
            Debug.LogError(
                "TrafficManager requires both lane prefabs.",
                this
            );

            return false;
        }

        if (lanes.Count >= maximumLaneCount)
            return false;

        int newLaneIndex = lanes.Count;

        bool shouldBeNorthbound =
            newLaneIndex % 2 == 0;

        RoadLane selectedPrefab =
            shouldBeNorthbound
                ? northboundLanePrefab
                : southboundLanePrefab;

        int directionLaneNumber;

        if (shouldBeNorthbound)
        {
            // Lane indices 2, 4, 6 become offsets 1, 2, 3.
            directionLaneNumber = newLaneIndex / 2;
        }
        else
        {
            // Lane indices 1, 3, 5 become offsets 1, 2, 3.
            directionLaneNumber = (newLaneIndex + 1) / 2;
        }

        float xDirection =
            shouldBeNorthbound ? 1f : -1f;

        Vector3 spawnPosition =
            startingLane.transform.position +
            Vector3.right *
            laneXOffset *
            directionLaneNumber *
            xDirection;

        RoadLane newLane = Instantiate(
            selectedPrefab,
            spawnPosition,
            selectedPrefab.transform.rotation,
            startingLane.transform.parent
        );

        newLane.transform.localScale =
            startingLane.transform.localScale;

        RegisterLane(newLane);

        return true;
    }

    private void ApplyCurrentUpgradesToLane(RoadLane lane)
    {
        if (lane == null || lane.TollBooth == null)
            return;

        lane.TollBooth.SetProcessingTime(
            currentProcessingTime
        );

        lane.TollBooth.SetMoneyPerCar(
            currentMoneyPerCar
        );
    }

    public void UnlockAutomation()
    {
        if (automationUnlocked)
            return;

        automationUnlocked = true;
        StartAutomation();
    }

    public void SetAutomationUnlocked(bool unlocked)
    {
        automationUnlocked = unlocked;

        if (automationUnlocked)
            StartAutomation();
        else
            StopAutomation();
    }

    public void StartAutomation()
    {
        if (!automationUnlocked || automationRoutine != null)
            return;

        automationRoutine =
            StartCoroutine(AutomationRoutine());
    }

    public void StopAutomation()
    {
        if (automationRoutine == null)
            return;

        StopCoroutine(automationRoutine);
        automationRoutine = null;
    }

    private IEnumerator AutomationRoutine()
    {
        while (automationUnlocked)
        {
            TrySpawnAutomaticCar();

            yield return new WaitForSeconds(spawnInterval);
        }

        automationRoutine = null;
    }

    public bool TrySpawnAutomaticCar()
    {
        RoadLane bestLane = FindBestLane();

        if (bestLane == null)
            return false;

        return bestLane.TrySpawnCar();
    }

    private RoadLane FindBestLane()
    {
        RoadLane bestLane = null;
        int shortestTrafficCount = int.MaxValue;

        foreach (RoadLane lane in lanes)
        {
            if (lane == null || !lane.CanAcceptCar)
                continue;

            if (lane.TrafficCount >= shortestTrafficCount)
                continue;

            shortestTrafficCount = lane.TrafficCount;
            bestLane = lane;
        }

        return bestLane;
    }

    public bool ReduceSpawnInterval(float amount)
    {
        if (amount <= 0f)
            return false;

        if (spawnInterval <= minimumSpawnInterval)
            return false;

        spawnInterval = Mathf.Max(
            minimumSpawnInterval,
            spawnInterval - amount
        );

        return true;
    }

    public void SetSpawnInterval(float value)
    {
        spawnInterval = Mathf.Clamp(
            value,
            minimumSpawnInterval,
            float.MaxValue
        );
    }

    public bool ReduceProcessingTimeForAllLanes(
        float amount,
        float minimumProcessingTime
    )
    {
        if (amount <= 0f)
            return false;

        if (currentProcessingTime <= minimumProcessingTime)
            return false;

        currentProcessingTime = Mathf.Max(
            minimumProcessingTime,
            currentProcessingTime - amount
        );

        SetProcessingTimeForAllLanes(
            currentProcessingTime
        );

        return true;
    }

    public void IncreaseMoneyPerCarForAllLanes(int amount)
    {
        if (amount <= 0)
            return;

        currentMoneyPerCar += amount;

        SetMoneyPerCarForAllLanes(
            currentMoneyPerCar
        );
    }

    public void SetProcessingTimeForAllLanes(float value)
    {
        currentProcessingTime = Mathf.Max(0.01f, value);

        foreach (RoadLane lane in lanes)
        {
            if (lane == null || lane.TollBooth == null)
                continue;

            lane.TollBooth.SetProcessingTime(
                currentProcessingTime
            );
        }
    }

    public void SetMoneyPerCarForAllLanes(int value)
    {
        currentMoneyPerCar = Mathf.Max(1, value);

        foreach (RoadLane lane in lanes)
        {
            if (lane == null || lane.TollBooth == null)
                continue;

            lane.TollBooth.SetMoneyPerCar(
                currentMoneyPerCar
            );
        }
    }

    public void BuildLanesUntilCount(int targetLaneCount)
    {
        targetLaneCount = Mathf.Clamp(
            targetLaneCount,
            1,
            maximumLaneCount
        );

        while (lanes.Count < targetLaneCount)
        {
            if (!BuildNextLane())
            {
                Debug.LogWarning(
                    $"Could not restore all lanes. " +
                    $"Current: {lanes.Count}, Target: {targetLaneCount}",
                    this
                );

                break;
            }
        }
    }
}