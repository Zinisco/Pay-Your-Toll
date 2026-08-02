using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarController carPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TollBooth tollBooth;

    [Header("Queue")]
    [SerializeField] private int maximumTotalCars = 25;

    [Header("Spawn Clearance")]
    [SerializeField] private LayerMask carLayerMask;

    [Tooltip("Size of the area that must be clear before another car spawns.")]
    [SerializeField]
    private Vector3 spawnCheckHalfExtents =
        new Vector3(1f, 1f, 2f);

    [SerializeField] private float spawnCheckInterval = 0.05f;

    [Header("Automation")]
    [SerializeField] private bool automationUnlocked;
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private float minimumSpawnInterval = 0.75f;

    private Coroutine automationRoutine;
    private Coroutine pendingSpawnRoutine;

    private int pendingSpawnRequests;

    public float SpawnInterval => spawnInterval;
    public float MinimumSpawnInterval => minimumSpawnInterval;
    public bool AutomationUnlocked => automationUnlocked;

    public int PendingSpawnRequests => pendingSpawnRequests;

    public int TotalRequestedCars =>
        tollBooth != null
            ? tollBooth.QueueCount + pendingSpawnRequests
            : pendingSpawnRequests;

    public int MaximumTotalCars => maximumTotalCars;

    public float CarsPerSecond
    {
        get
        {
            if (!automationUnlocked || spawnInterval <= 0f)
                return 0f;

            return 1f / spawnInterval;
        }
    }

    public float MaximumCarsPerSecond
    {
        get
        {
            if (minimumSpawnInterval <= 0f)
                return 0f;

            return 1f / minimumSpawnInterval;
        }
    }

    private void Start()
    {
        if (automationUnlocked)
            StartAutomation();
    }

    public bool TrySpawnCar()
    {
        if (carPrefab == null ||
            spawnPoint == null ||
            tollBooth == null)
        {
            return false;
        }

        int totalRequestedCars =
            tollBooth.QueueCount + pendingSpawnRequests;

        if (totalRequestedCars >= maximumTotalCars)
            return false;

        pendingSpawnRequests++;

        if (pendingSpawnRoutine == null)
        {
            pendingSpawnRoutine =
                StartCoroutine(ProcessPendingSpawns());
        }

        return true;
    }

    private IEnumerator ProcessPendingSpawns()
    {
        WaitForSeconds wait =
            new WaitForSeconds(spawnCheckInterval);

        while (pendingSpawnRequests > 0)
        {
            if (IsSpawnAreaBlocked())
            {
                yield return wait;
                continue;
            }

            SpawnCarImmediately();
            pendingSpawnRequests--;

            // Let Unity register the new collider before checking again.
            yield return null;
        }

        pendingSpawnRoutine = null;
    }

    private bool IsSpawnAreaBlocked()
    {
        return Physics.CheckBox(
            spawnPoint.position,
            spawnCheckHalfExtents,
            spawnPoint.rotation,
            carLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void SpawnCarImmediately()
    {
        CarController newCar = Instantiate(
            carPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        tollBooth.JoinQueue(newCar);
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
            StartCoroutine(SpawnCarsAutomatically());
    }

    public void StopAutomation()
    {
        if (automationRoutine == null)
            return;

        StopCoroutine(automationRoutine);
        automationRoutine = null;
    }

    private IEnumerator SpawnCarsAutomatically()
    {
        while (automationUnlocked)
        {
            TrySpawnCar();

            yield return new WaitForSeconds(spawnInterval);
        }

        automationRoutine = null;
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

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint == null)
            return;

        Gizmos.color = Color.cyan;

        Matrix4x4 previousMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            spawnPoint.position,
            spawnPoint.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            spawnCheckHalfExtents * 2f
        );

        Gizmos.matrix = previousMatrix;
    }


}