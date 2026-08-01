using System.Collections;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarController carPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TollBooth tollBooth;

    [Header("Spawn Limits")]
    [SerializeField] private float minimumSpawnInterval = 0.75f;

    [Header("Spawning")]
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private int maximumCarsInQueue = 8;

    [SerializeField] private bool spawnOnStart = true;

    public float SpawnInterval => spawnInterval;
    public float MinimumSpawnInterval => minimumSpawnInterval;

    private Coroutine spawnRoutine;

    private void Start()
    {
        if (spawnOnStart)
            StartSpawning();
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null)
            return;

        spawnRoutine = StartCoroutine(SpawnCars());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
            return;

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private IEnumerator SpawnCars()
    {
        while (true)
        {
            if (tollBooth.QueueCount < maximumCarsInQueue)
                SpawnCar();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCar()
    {
        if (carPrefab == null || spawnPoint == null || tollBooth == null)
            return;

        CarController newCar = Instantiate(
            carPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        tollBooth.JoinQueue(newCar);
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
}